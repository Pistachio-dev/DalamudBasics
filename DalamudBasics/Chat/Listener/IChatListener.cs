using Dalamud.Game.Text;
using System;

namespace DalamudBasics.Chat.Listener
{
    public interface IChatListener
    {
        public delegate void ChatMessageHandler(XivChatType type, string senderFullName, string message, DateTime receivedAt);

        void AddPreprocessedMessageListener(ChatMessageHandler listener);

        void InitializeAndRun(string pluginMessageMark, params XivChatType[] channelsToListenTo);
    }
}
