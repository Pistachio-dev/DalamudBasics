using Dalamud.Game.Text.SeStringHandling;

namespace DalamudBasics.Chat.Interpretation
{
    public interface IChatMessageInterpreter
    {
        bool TryParseDiceRoll(SeString message, out ChatDiceRoll chatDiceRoll);
    }
}