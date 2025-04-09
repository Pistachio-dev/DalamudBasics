using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using DalamudBasics.Chat.ClientOnlyDisplay;
using DalamudBasics.Chat.Interpretation.DiceReadingStrategy;
using DalamudBasics.Extensions;
using DalamudBasics.Logging;
using System;
using System.Text.RegularExpressions;

namespace DalamudBasics.Chat.Interpretation
{
    public class ChatMessageInterpreter : IChatMessageInterpreter
    {
        public ChatMessageInterpreter(IClientState clientState, IClientChatGui chatGui, ILogService logService)
        {
            this.clientState = clientState;
            this.chatGui = chatGui;
            this.logService = logService;
        }

        public static readonly Regex[] DiceRollRegex = [
            new Regex("Random! (?:\\(\\d+-(\\d+)\\))?"),
            new Regex("Lancer de dé (?:(\\d+) )?!"),
        ];

        private readonly IClientState clientState;
        private readonly IClientChatGui chatGui;
        private readonly ILogService logService;

        public bool TryParseDiceRoll(SeString message, out ChatDiceRoll chatDiceRoll)
        {
            chatDiceRoll = new ChatDiceRoll();
            if (clientState.ClientLanguage != Dalamud.Game.ClientLanguage.English)
            {
                logService.Warning($"Dice roll parsing is not supported for your client language.");

                return false;
            }

            var strategy = new DiceReadingStrategyEnglish();
            DiceRollType diceRollType = strategy.GetRollType(message);
            logService.Warning("Dice roll type " + diceRollType); // TODO: DELETE
            if (diceRollType == DiceRollType.None)
            {
                return false;
            }

            string rollerFullName = message.GetSenderFullName(clientState);
            if (diceRollType == DiceRollType.Dice && strategy.TryParseDiceRoll(message, out chatDiceRoll, rollerFullName))
            {
                return true;
            }

            if (diceRollType == DiceRollType.Random && strategy.TryParseRandomRoll(message, out chatDiceRoll, rollerFullName))
            {
                return true;
            }

            return false;
        }

        private static bool TryParseDiceRollEnglish(SeString message, out ChatDiceRoll chatDiceRoll)
        {
            chatDiceRoll = new ChatDiceRoll();
            var payload2 = message.GetPayload(2);
            if (payload2 != null && payload2.Type == PayloadType.RawText && payload2.GetText().Contains("Random!"))
            {
            }
            throw new NotImplementedException();
        }

        public static Match GetFirstSuccessfulMatch(Regex[] regexArray, string target)
        {
            foreach (Regex regex in regexArray)
            {
                Match match = regex.Match(target);
                if (match.Success)
                {
                    return match;
                }
            }

            return Match.Empty;
        }
    }
}
