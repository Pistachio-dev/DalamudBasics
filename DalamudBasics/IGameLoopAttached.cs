using Dalamud.Plugin.Services;

namespace DalamudBasics
{
    public interface IGameLoopLogicAttachedService
    {
        public void AttachToGameLogicLoop(IFramework framework);
    }
}
