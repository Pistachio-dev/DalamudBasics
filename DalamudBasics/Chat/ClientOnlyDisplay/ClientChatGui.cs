using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using DalamudBasics.Logging;
using DalamudBasics.Time;
using static Dalamud.Plugin.Services.IChatGui;

namespace DalamudBasics.Chat.ClientOnlyDisplay
{
    // Writes to chat client only (that is, nothing is actually sent to the server)
    public class ClientChatGui : IClientChatGui
    {
        private readonly IChatGui chatGui;
        private readonly ILogService logService;
        private readonly ITimeUtils timeUtils;

        public ClientChatGui(IChatGui chatGui, ILogService logService, ITimeUtils timeUtils)
        {
            this.chatGui = chatGui;
            this.logService = logService;
            this.timeUtils = timeUtils;
        }

        public void Print(string message)
        {
            chatGui.Print(message);
            logService.Info($"[ClientOnlyChat]{message}");
        }

        public void Print(string message, XivChatType chatType)
        {
            chatGui.Print(new XivChatEntry()
            {
                Message = message,
                Timestamp = (int)timeUtils.GetNowUnixTimestamp(false),
                Type = chatType
            });
        }

        // Home world is optional. Usually, the game only adds it for players not local to the current one.
        public void Print(string message, XivChatType chatType, string senderName)
        {
            chatGui.Print(new XivChatEntry()
            {
                Message = message,
                Timestamp = (int)timeUtils.GetNowUnixTimestamp(false),
                Type = chatType,
                Name = senderName,
            });
        }

        public void PrintError(string message)
        {
            chatGui.PrintError(message);
            logService.Info($"[ClientOnlyChat_AsError]{message}");
        }

        public void AddOnChatUIListener(OnMessageDelegate listener)
        {
            chatGui.ChatMessage += listener;
        }
    }
}
