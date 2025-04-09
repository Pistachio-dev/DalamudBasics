using Dalamud.Game.Text;
using Dalamud.Plugin.Services;

namespace DalamudBasics.Chat.ClientOnlyDisplay
{
    public interface IClientChatGui
    {
        void AddOnChatUIListener(IChatGui.OnMessageDelegate listener);

        void Print(string message);

        void Print(string message, XivChatType chatType);

        void Print(string message, XivChatType chatType, string senderName);

        void PrintError(string message);
    }
}
