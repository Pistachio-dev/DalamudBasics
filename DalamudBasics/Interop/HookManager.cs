using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using DalamudBasics.DiceRolling;
using DalamudBasics.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using FFXIVClientStructs.STD;
using Lumina.Excel.Sheets;
using System;

/// This class was taken from https://github.com/Infiziert90/DeathRoll
namespace DalamudBasics.Interop
{
    public unsafe class HookManager : IDisposable
    {
        private readonly IGameInteropProvider interopProvider;
        private readonly IDataManager dataManager;
        private readonly ILogService logService;
        private readonly DiceRollManager diceRollManager;

        [Signature("E8 ?? ?? ?? ?? EB ?? 45 33 C9 4C 8B C6", DetourName = nameof(RandomPrintLogDetour))]
        private Hook<RandomPrintLogDelegate>? RandomPrintLogHook { get; set; }

        private delegate void RandomPrintLogDelegate(RaptureLogModule* module, int logMessageId, byte* playerName, byte sex, StdDeque<TextParameter>* parameter, byte flags, ushort homeWorldId);

        [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 55 57 41 54 41 55 41 56 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 0F B7 75", DetourName = nameof(DicePrintLogDetour))]
        private Hook<DicePrintLogDelegate>? DicePrintLogHook { get; set; }

        private delegate void DicePrintLogDelegate(RaptureLogModule* module, ushort chatType, byte* userName, void* unused, ushort worldId, ulong accountId, ulong contentId, ushort roll, ushort outOf, uint entityId, byte ident);

        public HookManager(IGameInteropProvider interopProvider, IDataManager dataManager, ILogService logService, DiceRollManager diceRollManager)
        {
            this.interopProvider = interopProvider;
            this.dataManager = dataManager;
            this.logService = logService;
            this.diceRollManager = diceRollManager;
            interopProvider.InitializeFromAttributes(this);

            RandomPrintLogHook?.Enable();
            DicePrintLogHook?.Enable();
        }

        public void Dispose()
        {
            RandomPrintLogHook?.Dispose();
            DicePrintLogHook?.Dispose();
        }

        private void RandomPrintLogDetour(RaptureLogModule* module, int logMessageId, byte* playerName, byte sex, StdDeque<TextParameter>* parameter, byte flags, ushort homeWorldId)
        {
            if (logMessageId != 856 && logMessageId != 3887)
            {
                RandomPrintLogHook!.Original(module, logMessageId, playerName, sex, parameter, flags, homeWorldId);
                return;
            }

            try
            {
                var name = MemoryHelper.ReadStringNullTerminated((nint)playerName);
                var world = dataManager.GetExcelSheet<World>()!.GetRow(homeWorldId)!;
                var fullName = $"{name}@{world.Name}";

                var roll = (*parameter)[1].IntValue;
                var outOf = logMessageId == 3887 ? (*parameter)[2].IntValue : 0;

                diceRollManager.InvokeDiceRollEvent(fullName, DiceRollType.Random, roll, outOf);
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Unable to process random roll");
            }

            RandomPrintLogHook!.Original(module, logMessageId, playerName, sex, parameter, flags, homeWorldId);
        }

        private void DicePrintLogDetour(RaptureLogModule* module, ushort chatType, byte* playerName, void* unused, ushort worldId, ulong accountId, ulong contentId, ushort roll, ushort outOf, uint entityId, byte ident)
        {
            try
            {                
                var name = MemoryHelper.ReadStringNullTerminated((nint)playerName);
                var world = dataManager.GetExcelSheet<World>()!.GetRow(worldId)!;
                var fullName = $"{name}@{world.Name}";

                diceRollManager.InvokeDiceRollEvent(fullName, DiceRollType.Dice, roll, outOf);
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Unable to process dice roll");
            }

            DicePrintLogHook!.Original(module, chatType, playerName, unused, worldId, accountId, contentId, roll, outOf, entityId, ident);
        }
    }
}
