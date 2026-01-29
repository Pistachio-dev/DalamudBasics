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
        private string pluginMessageMark = "[C]";
        private readonly IClientChatGui clientChatGui;
        private readonly IClientState gameClient;
        private readonly ITimeUtils timeUtils;
        private readonly ILogService logService;
        private readonly IObjectTable objectTable;
        private List<XivChatType> channelsToListenTo = new();

        private event ChatMessageHandler? OnChatMessage;

        public ChatListener(IClientChatGui chatGui, IClientState gameClient, ITimeUtils timeUtils, ILogService logService, IObjectTable objectTable)
        {
            this.clientChatGui = chatGui;
            this.gameClient = gameClient;
            this.timeUtils = timeUtils;
            this.logService = logService;
            this.objectTable = objectTable;
        }

        /// <summary>
        /// Initializes and attaches to the game the chat listener.
        /// </summary>
        /// <param name="pluginMessageMark"></param>
        public void InitializeAndRun(string pluginMessageMark, bool attachPreprocessor, params XivChatType[] channelsToListenTo)
        {
            this.pluginMessageMark = pluginMessageMark;
            this.channelsToListenTo.AddRange(channelsToListenTo);
            AttachToGameChat(attachPreprocessor);
        }

        public void AddPreprocessedMessageListener(ChatMessageHandler listener)
        {
            OnChatMessage += listener;
        }

        private void AttachToGameChat(bool attachPreprocessor)
        {
            if (attachPreprocessor)
            {
                clientChatGui.AddOnChatUIListener(PropagateToCustomEvent);
            }
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
                logService.Debug($"Message sent by the plugin ignored: " + messageAsString);
                return;
            }

            string senderFullName = GetFullPlayerNameFromSenderData(sender);

            DateTime localTime = timeUtils.GetLocalDateTime();

            //logService.Debug($"Message processed and triggering custom event: " + messageAsString);
            OnChatMessage?.Invoke(type, senderFullName, messageAsString, localTime);
        }

        private string GetFullPlayerNameFromSenderData(SeString messageSender)
        {
            return messageSender.GetSenderFullName(objectTable);
        }
    }
}
