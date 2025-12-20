using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using DalamudBasics.Chat.Interpretation;
using DalamudBasics.Extensions;
using DalamudBasics.Logging;

namespace DalamudBasics.Debugging
{
    public class StringDebugUtils
    {
        private readonly ILogService logService;
        private readonly IChatMessageInterpreter chatMessageInterpreter;
        private readonly IObjectTable objectTable;

        public StringDebugUtils(ILogService logService, IChatMessageInterpreter chatMessageInterpreter, IObjectTable objectTable)
        {
            this.logService = logService;
            this.chatMessageInterpreter = chatMessageInterpreter;
            this.objectTable = objectTable;
        }

        public void DumpSeString(SeString s)
        {
            int counter = 0;
            foreach (Payload payload in s.Payloads)
            {
                string embeddedInfoType = payload.Type.ToString();
                string text = "Unreadable";
                if (payload is ITextProvider)
                {
                    ITextProvider textProvider = (ITextProvider)payload;
                    text = textProvider.Text;
                }
                
                string output = $"Payload {counter} Type: {embeddedInfoType} Text: \"{text}\"";
                logService.Info(output);
                counter++;
            }
        }

        public void DumpAllReceivedMessages(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            logService.Info("------------------------------------------------------------------------------------------------------------");
            logService.Info($"Type: {type} Timestamp: {timestamp} IsHandled: {isHandled}");
            logService.Info("Sender as interpreted: " + sender.GetSenderFullName(objectTable));
            logService.Info("Sender SeString dump---------------------------");
            DumpSeString(sender);
            logService.Info("Message SeString dump---------------------------");
            DumpSeString(message);
            logService.Info("------------------------------------------------------------------------------------------------------------");
        }

        public void TestDiceRollParsing(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!chatMessageInterpreter.TryParseDiceRoll(message, out ChatDiceRoll result))
            {
                logService.Info("No roll detected");
                return;
            }

            if (result.IsRangeLimited)
            {
                logService.Info($"Roll read: {result.RolledNumber} ({result.LowerLimit} to {result.UpperLimit}, by {result.RollingPlayer})");

                return;
            }

            logService.Info($"Roll read: {result.RolledNumber}, by {result.RollingPlayer}");
        }
    }
}
