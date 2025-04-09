using DalamudBasics.Logging;

namespace DalamudBasics.DiceRolling
{
    public class DiceRollManager
    {
        private readonly ILogService logService;

        public DiceRollManager(ILogService logService)
        {
            this.logService = logService;
            OnDiceRoll += LogRoll;
        }

        public void LogRoll(DiceRoll roll)
        {
            logService.Info(roll.ToString());
        }

        public delegate void DiceRollHandler(DiceRoll roll);

        public event DiceRollHandler OnDiceRoll;

        public void InvokeDiceRollEvent(string fullPlayerName, DiceRollType type, int result, int outOf)
        {
            var roll = new DiceRoll(type, fullPlayerName, result, outOf);
            OnDiceRoll.Invoke(roll);
        }
    }
}
