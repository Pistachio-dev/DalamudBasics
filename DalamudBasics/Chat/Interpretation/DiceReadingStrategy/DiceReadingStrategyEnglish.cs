using Dalamud.Game.Text.SeStringHandling;
using DalamudBasics.Extensions;
using System.Linq;
using System.Text.RegularExpressions;

namespace DalamudBasics.Chat.Interpretation.DiceReadingStrategy
{
    public class DiceReadingStrategyEnglish
    {
        public DiceRollType GetRollType(SeString message)
        {
            var payload1 = message.GetPayload(1);
            if (payload1?.Type == PayloadType.RawText && payload1.GetText().Contains("Random!"))
            {
                return DiceRollType.Dice;
            }

            var payload0 = message.GetPayload(0);
            if (payload0?.Type == PayloadType.RawText && payload0.GetText().Contains("Random!"))
            {
                return DiceRollType.Random;
            }

            return DiceRollType.None;
        }

        public bool TryParseDiceRoll(SeString message, out ChatDiceRoll chatDiceRoll, string rollerFullName)
        {
            chatDiceRoll = new ChatDiceRoll();
            chatDiceRoll.Type = DiceRollType.Dice;
            chatDiceRoll.RollingPlayer = rollerFullName;
            if (message.Payloads.Count != 5)
            {
                return false;
            }

            var regex = new Regex("Random! (?:\\(\\d+-(\\d+)\\))?");
            var match = regex.Match(message.GetPayload(1)?.GetText() ?? string.Empty);
            if (!match.Success)
            {
                return false;
            }

            if (match.Groups[1].Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int maxRange))
                {
                    chatDiceRoll.IsRangeLimited = true;
                    chatDiceRoll.SetRange(1, maxRange);
                }
            }

            if (!int.TryParse(message.GetPayload(3)?.GetText() ?? string.Empty, out int rolledNumber))
            {
                return false;
            }

            chatDiceRoll.RolledNumber = rolledNumber;
            return true;
        }

        public bool TryParseRandomRoll(SeString message, out ChatDiceRoll chatDiceRoll, string hostFullName)
        {
            chatDiceRoll = new ChatDiceRoll();
            chatDiceRoll.Type = DiceRollType.Random;

            if (message.Payloads.Count == 3)
            {
                return TryParseRollRandomByHost(message, ref chatDiceRoll, hostFullName);
            }
            if (message.Payloads.Count == 8 || message.Payloads.Count == 7)
            {
                return TryParseRandomRollByNotHost(message, ref chatDiceRoll, hostFullName);
            }

            return false;
        }

        private bool TryParseRollRandomByHost(SeString message, ref ChatDiceRoll chatDiceRoll, string hostFullName)
        {
            chatDiceRoll.RollingPlayer = hostFullName;
            if (message.GetPayload(0)?.GetText() != "Random! You roll a ")
            {
                return false;
            }

            return TryParseRollNumberAndLimit(message.GetPayload(2), ref chatDiceRoll);
        }

        private bool TryParseRandomRollByNotHost(SeString message, ref ChatDiceRoll chatDiceRoll, string hostFullName)
        {
            if (message.GetPayload(0)?.GetText() != "Random! ")
            {
                return false;
            }

            string playerName = message.GetPayload(2)?.GetText() ?? string.Empty;
            string playerWorld;

            if (message.Payloads.Count == 8)
            {
                var worldRegex = new Regex("(\\w+)? rolls a ");
                var match = worldRegex.Match(message.GetPayload(5)?.GetText() ?? string.Empty);
                playerWorld = match.Groups[1].Value;
            }
            else
            {
                playerWorld = hostFullName.Split('@')[1];
            }

            chatDiceRoll.RollingPlayer = $"{playerName}@{playerWorld}";
            Payload payloadWithRolledNumber = message.Payloads.Last();
            return TryParseRollNumberAndLimit(payloadWithRolledNumber, ref chatDiceRoll);
        }

        private bool TryParseRollNumberAndLimit(Payload? payloadWithTheNumbers, ref ChatDiceRoll chatDiceRoll)
        {
            var regex = new Regex("(\\d+)[\\s\\.](?:\\(out of (\\d+)\\))?");
            var match = regex.Match(payloadWithTheNumbers?.GetText() ?? string.Empty);
            if (!match.Success)
            {
                return false;
            }

            if (!int.TryParse(match.Groups[1].Value, out int rolledNumber))
            {
                return false;
            }

            chatDiceRoll.RolledNumber = rolledNumber;
            if (int.TryParse(match.Groups[2].Value, out int upperLimit))
            {
                chatDiceRoll.SetRange(1, upperLimit);
            }

            return true;
        }
    }
}
