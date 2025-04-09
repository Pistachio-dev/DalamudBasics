using Dalamud.Game;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using DalamudBasics.Chat.ClientOnlyDisplay;
using DalamudBasics.Configuration;
using DalamudBasics.Extensions;
using DalamudBasics.Logging;
using DalamudBasics.Targeting;
using System;
using System.Collections.Concurrent;

namespace DalamudBasics.Chat.Output
{
    public class ChatOutput : IChatOutput
    {
        private const int ExtraDelayOnRetry = 500;

        private string? waterMark = null;
        private bool initialized = false;
        private ConcurrentQueue<ChatOutputQueuedMessage> chatQueue = new();
        private ConcurrentQueue<ChatOutputQueuedMessage> retryQueue = new();
        private DateTime lastTimeChatWasWritten = DateTime.MinValue;
        private ChatOutputQueuedMessage? lastMessageSent = null;
        private readonly IConfiguration configuration;
        private readonly ILogService logService;
        private readonly IClientChatGui chatGui;
        private readonly IClientState clientState;
        private readonly ITargetingService targetingService;

        private XivChatType DefaultOutputChatType
        {
            get
            {
                return configuration.DefaultOutputChatType;
            }
        }

        public ChatOutput(IConfiguration configuration, ILogService logService, IClientChatGui chatGui, IClientState clientState, ITargetingService targetingService)
        {
            this.configuration = configuration;
            this.logService = logService;
            this.chatGui = chatGui;
            this.clientState = clientState;
            this.targetingService = targetingService;
        }

        public void WriteCommand(string command, int delay = 0, string? targetFullName = null)
        {
            if (!command.StartsWith("/"))
            {
                command = "/" + command;
            }

            EnqueueMessage(command, XivChatType.None, delay, targetFullName);
        }

        public void WriteChat(string message, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0, string? targetFullName = null)
        {
            if (waterMark != null && chatChannel != XivChatType.TellIncoming && chatChannel != XivChatType.TellOutgoing)
            {
                message = waterMark + message;
            }

            EnqueueMessage(message, chatChannel, minSpacingBeforeInMs, targetFullName);
        }

        private void EnqueueMessage(string message, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0, string? targetFullName = null)
        {
            if (!initialized)
            {
                NotifyNotAttachedToGame();
                return;
            }

            chatQueue.Enqueue(new ChatOutputQueuedMessage(message, chatChannel, minSpacingBeforeInMs, targetFullName));
        }

        public void SendTell(string message, string playerFullNameWithWorld, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0)
        {
            var split = playerFullNameWithWorld.Split("@");
            string player = split[0];
            string world = split.Length > 1 ? split[1] : string.Empty;
            SendTell(message, player, world, chatChannel, minSpacingBeforeInMs);
        }

        public void SendTell(string message, string playerFullName, string playerHomeWorld, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0)
        {
            if (!initialized)
            {
                NotifyNotAttachedToGame();
                return;
            }

            string messageWithRecipient = $"{playerFullName}@{playerHomeWorld} {message}";
            WriteChat(messageWithRecipient, chatChannel ?? XivChatType.TellOutgoing, minSpacingBeforeInMs);
        }

        private void NotifyNotAttachedToGame()
        {
            logService.Error($"You forgot to call {this.GetType()}.{nameof(InitializeAndAttachToGameLogicLoop)}!");
        }

        // Leave waterMark as null if you don't want to attach a mark to your messages
        public void InitializeAndAttachToGameLogicLoop(IFramework framework, string? waterMark = null)
        {
            this.waterMark = waterMark;
            framework.Update += Tick;
            chatGui.AddOnChatUIListener(MessageNotSentDetector);
            initialized = true;
        }

        public void MessageNotSentDetector(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (type != XivChatType.ErrorMessage || message.Payloads.Count != 1 || message.Payloads[0].Type != PayloadType.RawText || sender.Payloads.Count != 0)
            {
                return;
            }

            string text = message.Payloads[0].GetText();
            if (IsMessageNotSentErrorMessage(text) && lastMessageSent != null)
            {
                if (IsLimitedChatChannel(lastMessageSent.ChatChannel) && (DateTime.Now - lastTimeChatWasWritten) < TimeSpan.FromSeconds(2))
                {
                    logService.Warning($"Message \"{lastMessageSent.Message}\" could not be sent. Retrying.");
                    lastMessageSent.SpacingBeforeInMs += ExtraDelayOnRetry;
                    retryQueue.Enqueue(lastMessageSent);
                }
                else
                {
                    logService.Info($"Message could not be sent, but it should not be one of mine.");
                }
            }
            else
            {
                logService.Debug("Message did not match the whole thing.");
            }
        }

        private bool IsMessageNotSentErrorMessage(string text)
        {
            switch (clientState.ClientLanguage)
            {
                case ClientLanguage.English:
                    return text.Equals("Your message was not heard. You must wait before using /tell, /say, /yell, or /shout again.");

                case ClientLanguage.French:
                    return text.Equals("Impossible de murmurer/dire/crier/hurler plusieurs fois de suite.");

                case ClientLanguage.German:
                    return text.Equals("Nachricht wurde nicht gesendet. Rufen, Schreien, Sagen und Flüstern ist nicht mehrmals hintereinander möglich. ");

                case ClientLanguage.Japanese:
                    return text.Equals("送信できませんでした。Tell/Say/Yell/Shoutは連続して実行できません。");
            }

            return false;
        }

        protected XivChatType GetOutputTypeForTell()
        {
            return configuration.DefaultOutputChatType == XivChatType.Echo ? XivChatType.Echo : XivChatType.TellOutgoing;
        }

        private void Tick(IFramework framework)
        {
            bool arellRetryMessagesProcessed = SendMessagesFromQueueAllowedOnThisTick(retryQueue);
            if (arellRetryMessagesProcessed)
            {
                SendMessagesFromQueueAllowedOnThisTick(chatQueue);
            }
        }

        // Returns true if all messages were processed, or false if we are waiting on one.
        private bool SendMessagesFromQueueAllowedOnThisTick(ConcurrentQueue<ChatOutputQueuedMessage> queue)
        {
            while (queue.TryPeek(out ChatOutputQueuedMessage? nextChatPayload))
            {
                if (IsLimitedChatChannel(nextChatPayload.ChatChannel))
                {
                    if (nextChatPayload.SpacingBeforeInMs < configuration.LimitedChatChannelsMessageDelayInMs)
                    {
                        nextChatPayload.SpacingBeforeInMs = configuration.LimitedChatChannelsMessageDelayInMs + new Random().Next(0, 150);
                    }
                }

                if ((DateTime.Now - lastTimeChatWasWritten).TotalMilliseconds < nextChatPayload.SpacingBeforeInMs)
                {
                    return false;
                }

                if (queue.TryDequeue(out nextChatPayload!))
                {
                    ActuallyWriteChat(nextChatPayload);
                    lastTimeChatWasWritten = DateTime.Now;
                }
            }

            return true;
        }

        private void ActuallyWriteChat(ChatOutputQueuedMessage payload)
        {
            if (payload.Message.IsNullOrEmpty())
            {
                return;
            }
            if (payload.ChatChannel == null)
            {
                payload.ChatChannel = DefaultOutputChatType;
            }
            if (payload.TargetFullName != null)
            {
                bool successfulTargeting = targetingService.TargetPlayer(payload.TargetFullName);
                if (!successfulTargeting)
                {
                    logService.Warning($"Could not target player '{payload.TargetFullName}', skipping command");
                }
            }

            try
            {
                var messagePrefix = GetChannelPrefix(payload.ChatChannel);

                lastMessageSent = payload;

                var sanitizedText = ECommons.Automation.Chat.Instance.SanitiseText(payload.Message);
                string fullChatString = sanitizedText;
                if (!messagePrefix.IsNullOrEmpty())
                {
                    fullChatString = $"{messagePrefix} {sanitizedText}";
                }

                ECommons.Automation.Chat.Instance.SendMessage(fullChatString);
                if (configuration.LogOutgoingChatOutput)
                {
                    logService.Info("[Chat]" + fullChatString);
                }
            }
            catch (Exception ex)
            {
                chatGui.PrintError("Invalid chat channel (or some odd error) for message: " + payload.Message + " with prefix " + payload.ChatChannel.ToString());
                logService.Error(ex, $"Error when sending message \"{payload.Message}\" with prefix \"{payload.ChatChannel}\"");
            }

            if (payload.TargetFullName != null)
            {
                targetingService.ClearTarget();
            }
        }

        private string GetChannelPrefix(XivChatType? chatChannel)
        {
            if (chatChannel == null)
            {
                return string.Empty;
            }

            return chatChannel switch
            {
                XivChatType.Echo => "/echo",
                XivChatType.Party => "/p",
                XivChatType.Alliance => "/a",
                XivChatType.Say => "/s",
                XivChatType.TellOutgoing => "/tell",
                XivChatType.Yell => "/yell",
                XivChatType.Shout => "/shout",
                XivChatType.CrossLinkShell1 => "/cwlinkshell1",
                XivChatType.CrossLinkShell2 => "/cwlinkshell2",
                XivChatType.CrossLinkShell3 => "/cwlinkshell3",
                XivChatType.CrossLinkShell4 => "/cwlinkshell4",
                XivChatType.CrossLinkShell5 => "/cwlinkshell5",
                XivChatType.CrossLinkShell6 => "/cwlinkshell6",
                XivChatType.CrossLinkShell7 => "/cwlinkshell7",
                XivChatType.CrossLinkShell8 => "/cwlinkshell8",
                XivChatType.Ls1 => "/linkshell1",
                XivChatType.Ls2 => "/linkshell2",
                XivChatType.Ls3 => "/linkshell3",
                XivChatType.Ls4 => "/linkshell4",
                XivChatType.Ls5 => "/linkshell5",
                XivChatType.Ls6 => "/linkshell6",
                XivChatType.Ls7 => "/linkshell7",
                XivChatType.Ls8 => "/linkshell8",

                XivChatType.None => string.Empty, // I use this to trigger emotes. The actual XivChatType.Emote is the displayed message in /emotelog
                _ => throw new Exception("Unsupported output chat channel.")
            };
        }

        private bool IsLimitedChatChannel(XivChatType? chatEntryChannel)
        {
            return (chatEntryChannel is XivChatType.Shout or XivChatType.Yell or XivChatType.Say or XivChatType.TellOutgoing)
                || (chatEntryChannel == null && configuration.DefaultOutputChatType is XivChatType.Shout or XivChatType.Yell or XivChatType.Say or XivChatType.TellOutgoing);
        }
    }
}
