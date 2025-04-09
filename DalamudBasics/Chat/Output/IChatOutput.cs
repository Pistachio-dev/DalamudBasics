using Dalamud.Game.Text;
using Dalamud.Plugin.Services;

namespace DalamudBasics.Chat.Output
{
    public interface IChatOutput
    {
        void WriteChat(string message, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0, string? targetFullName = null);

        void SendTell(string message, string playerFullName, string playerHomeWorld, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0);

        void InitializeAndAttachToGameLogicLoop(IFramework framework, string? waterMark = null);

        void WriteCommand(string command, int delay = 0, string? targetFullName = null);

        void SendTell(string message, string playerFullNameWithWorld, XivChatType? chatChannel = null, int minSpacingBeforeInMs = 0);
    }
}
