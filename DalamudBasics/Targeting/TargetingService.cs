using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using DalamudBasics.Chat.ClientOnlyDisplay;
using DalamudBasics.Extensions;
using DalamudBasics.Logging;
using System.Collections.Generic;
using System.Linq;
using PlayerReferenceMap = System.Collections.Generic.Dictionary<string, Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>;

namespace DalamudBasics.Targeting
{
    internal class TargetingService : ITargetingService
    {
        private readonly ITargetManager dalamudTargetManager;
        private readonly IObjectTable gameObjectTable;
        private readonly ILogService logService;
        private readonly IClientChatGui chatGui;
        private PlayerReferenceMap playerRefs;

        public TargetingService(ITargetManager targetManager, IObjectTable gameObjectTable, ILogService logService, IClientChatGui chatGui)
        {
            playerRefs = [];
            this.dalamudTargetManager = targetManager;
            this.gameObjectTable = gameObjectTable;
            this.logService = logService;
            this.chatGui = chatGui;
        }

        public void RemovePlayerReference(string playerFullName)
        {
            playerRefs.Remove(playerFullName);
        }

        public bool TrySaveTargetPlayerReference(out IPlayerCharacter? reference)
        {
            var targetName = GetTargetName();
            reference = null;
            if (string.IsNullOrEmpty(targetName))
            {
                chatGui.PrintError("Cannot save target player reference. Nothing is targeted, or it is not a player.");
                logService.Info("Failed attemp to save targeted reference: not a player");

                return false;
            }

            // At this point we know it is a player character, valid, not null.
            reference = (IPlayerCharacter)dalamudTargetManager.Target!;
            playerRefs[targetName] = reference;
            logService.Debug("Stored reference for player " + targetName);

            return true;
        }

        public void ClearTarget()
        {
            dalamudTargetManager.Target = null;
        }

        public bool TargetPlayer(string fullPlayerName)
        {
            var playerCharacter = playerRefs.GetValueOrDefault(fullPlayerName);
            if (playerCharacter == null)
            {
                chatGui.Print("Could not find the stored player reference. Trying to find if it is still spawned for you.");

                (string name, string world) = fullPlayerName.SplitFullName();
                playerCharacter = GetPlayerReferenceFromObjectTable(name, world);
                if (playerCharacter == null)
                {
                    chatGui.PrintError($"Can't find {fullPlayerName} spawned anywhere.");
                    logService.Info($"Failed to find player in object table: " + fullPlayerName);

                    return false;
                }

                playerRefs[BuildFullPlayerName(playerCharacter)] = playerCharacter;
                chatGui.Print($"{fullPlayerName} found, recreating reference.");
                logService.Info($"Failed to find player in object table: " + fullPlayerName);
            }

            dalamudTargetManager.Target = playerCharacter;
            logService.Info($"Targeting player: " + fullPlayerName);

            return VerifyTargeting(fullPlayerName);
        }

        public string GetTargetName()
        {
            var target = dalamudTargetManager.Target;
            if (target == null || target is not IPlayerCharacter pc || pc.HomeWorld.ValueNullable == null)
                return string.Empty;

            return BuildFullPlayerName(pc);
        }

        public bool IsTargetingAPlayer()
        {
            var target = dalamudTargetManager.Target;
            return target != null && target is IPlayerCharacter;
        }

        private IPlayerCharacter? GetPlayerReferenceFromObjectTable(string name, string world)
        {
            return (IPlayerCharacter?)gameObjectTable.FirstOrDefault(o => o is IPlayerCharacter player && player.Matches(name, world));
        }

        private static string BuildFullPlayerName(IPlayerCharacter pc)
        {
            return pc.GetFullName();
        }

        private bool VerifyTargeting(string fullPlayerName)
        {
            return fullPlayerName == GetTargetName();
        }
    }
}
