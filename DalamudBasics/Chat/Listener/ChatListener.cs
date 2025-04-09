using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using DalamudBasics.Chat.ClientOnlyDisplay;
using DalamudBasics.Extensions;
using DalamudBasics.Logging;
using DalamudBasics.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using static DalamudBasics.Chat.Listener.IChatListener;

namespace DalamudBasics.Chat.Listener
{
    /// <summary>
    /// Provides an <see cref="OnChatMessage"/> event that will be raised on every game chat received.
    /// </summary>
    internal class ChatListener : IChatListener
    {
        private string pluginMessageMark;
        private readonly IClientChatGui clientChatGui;
        private readonly IClientState gameClient;
        private readonly ITimeUtils timeUtils;
        private readonly ILogService logService;
        private List<XivChatType> channelsToListenTo = new();

        private event ChatMessageHandler OnChatMessage;

        public ChatListener(IClientChatGui chatGui, IClientState gameClient, ITimeUtils timeUtils, ILogService logService)
        {
            this.clientChatGui = chatGui;
            this.gameClient = gameClient;
            this.timeUtils = timeUtils;
            this.logService = logService;
        }

        /// <summary>
        /// Initializes and attaches to the game the chat listener.
        /// </summary>
        /// <param name="pluginMessageMark"></param>
        public void InitializeAndRun(string pluginMessageMark, params XivChatType[] channelsToListenTo)
        {
            this.pluginMessageMark = pluginMessageMark;
            this.channelsToListenTo.AddRange(channelsToListenTo);
            AttachToGameChat();
        }

        public void AddPreprocessedMessageListener(ChatMessageHandler listener)
        {
            OnChatMessage += listener;
        }

        private void AttachToGameChat()
        {
            clientChatGui.AddOnChatUIListener(PropagateToCustomEvent);
        }

        private void PropagateToCustomEvent(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (channelsToListenTo.Any() && !channelsToListenTo.Contains(type))
            {
                return;
            }

            string messageAsString = message.ToString();
            if (messageAsString.Contains(pluginMessageMark, StringComparison.OrdinalIgnoreCase))
            {
                logService.Info($"Message sent by the plugin ignored: " + messageAsString);
                return;
            }

            string senderFullName = GetFullPlayerNameFromSenderData(sender);

            DateTime localTime = timeUtils.GetLocalDateTime();

            logService.Info($"Message processed and triggering custom event: " + messageAsString);
            OnChatMessage?.Invoke(type, senderFullName, messageAsString, localTime);
        }

        private string GetFullPlayerNameFromSenderData(SeString messageSender)
        {
            return messageSender.GetSenderFullName(gameClient);
        }
    }
}
