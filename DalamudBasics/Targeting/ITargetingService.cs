using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

namespace DalamudBasics.Targeting
{
    public interface ITargetingService
    {
        void ClearTarget();
        IGameObject? GetTarget();
        string GetTargetName();

        bool IsTargetingAPlayer();

        void RemovePlayerReference(string playerFullName);

        bool TargetPlayer(string fullPlayerName);

        bool TrySaveTargetPlayerReference(out IPlayerCharacter? reference);
    }
}
