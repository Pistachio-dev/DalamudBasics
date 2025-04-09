namespace DalamudBasics.DiceRolling
{
    public class DiceRoll
    {
        public DiceRoll(DiceRollType rollType, string playerFullName, int result, int outOf)
        {
            Type = rollType;
            PlayerFullName = playerFullName;
            RollResult = result;
            OutOf = outOf;
            if (outOf == 0) { OutOf = 999; }
        }

        public override string ToString()
        {
            return $"{PlayerFullName}-/{Type}: {RollResult} out of {OutOf}";
        }

        public DiceRollType Type { get; set; }
        public int RollResult { get; set; }
        public int OutOf { get; set; }
        public string PlayerFullName { get; set; }
    }
}
