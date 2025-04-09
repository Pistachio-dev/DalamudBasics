using Dalamud.Game.Text;

namespace DalamudBasics.Chat.Output
{
    internal class ChatOutputQueuedMessage
    {
        public string Message { get; set; }

        public XivChatType? ChatChannel { get; set; }

        public int SpacingBeforeInMs { get; set; }

        public string? TargetFullName { get; set; }

        public ChatOutputQueuedMessage(string message, XivChatType? chatOutputType = null, int spacingBeforeInMs = 0, string? targetFullName = null)
        {
            Message = message;
            this.ChatChannel = chatOutputType;
            SpacingBeforeInMs = spacingBeforeInMs;
            TargetFullName = targetFullName;
        }
    }
}
