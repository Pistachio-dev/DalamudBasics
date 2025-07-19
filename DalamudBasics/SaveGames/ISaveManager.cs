using System;

namespace DalamudBasics.SaveGames
{
    public interface ISaveManager<T> where T : new()
    {
        DateTime? LastTimeSaved { get; }

        T GetCharacterSaveInMemory();
        T? LoadCharacterSave();
        T? LoadSave(bool characterDepenent = false);
        void WriteCharacterSave(T gameState);
        void WriteCharacterSave();
        void WriteSave(T gameState, bool characterDepenent = false);
    }
}
