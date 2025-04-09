namespace DalamudBasics.Chat.Interpretation
{
    public class ChatDiceRoll
    {
        public int RolledNumber { get; set; }
        public bool IsRangeLimited { get; set; }
        public int LowerLimit { get; set; } = -1;
        public int UpperLimit { get; set; } = -1;
        public DiceRollType Type { get; set; }
        public string RollingPlayer { get; set; }

        public void SetRange(int min, int max)
        {
            IsRangeLimited = true;
            LowerLimit = min;
            UpperLimit = max;
        }
    }
}
