using DalamudBasics.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DalamudBasics.SaveGames
{
    public class SaveManager<T> : ISaveManager<T> where T : new()
    {
        private readonly string saveFileRoute;
        private readonly ILogService logService;
        private DateTime? lastTimeSaved;

        public DateTime? LastTimeSaved
        {
            get
            {
                if (lastTimeSaved == null)
                {
                    return File.Exists(saveFileRoute) ? File.GetLastWriteTime(saveFileRoute) : null;
                }

                return lastTimeSaved;
            }
            private set { lastTimeSaved = value; }
        }

        public SaveManager(string saveFileRoute, ILogService logService)
        {
            this.saveFileRoute = saveFileRoute;
            this.logService = logService;
        }

        public T? LoadSave()
        {
            return LoadObjectFromFile<T>();
        }

        public void WriteSave(T gameState)
        {
            try
            {
                LastTimeSaved = DateTime.Now;
                SaveObjectToFile(gameState, saveFileRoute);
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Error on game save.");
            }
        }

        private T? LoadObjectFromFile<T>()
        {
            if (!File.Exists(saveFileRoute))
            {
                return default(T);
            }

            string jsonText = File.ReadAllText(saveFileRoute);
            T result = JsonSerializer.Deserialize<T>(jsonText) ?? throw new Exception($"Error loading file {saveFileRoute}.");

            return result;
        }

        private void SaveObjectToFile<T>(T obj, string fileName)
        {
            try
            {
                string jsonText = JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                });

                File.WriteAllText(saveFileRoute, jsonText);
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Error when trying to save game state");
            }
        }
    }
}
