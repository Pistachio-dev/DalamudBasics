using System;

namespace DalamudBasics.SaveGames
{
    public interface ISaveManager<T> where T : new()
    {
        DateTime? LastTimeSaved { get; }

        T? LoadSave();

        void WriteSave(T gameState);
    }
}
