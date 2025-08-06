using Dalamud.Plugin.Services;
using DalamudBasics.Extensions;
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
        private readonly IClientState clientState;
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

        public SaveManager(string saveFileRoute, ILogService logService, IClientState clientState)
        {
            this.saveFileRoute = saveFileRoute;
            this.logService = logService;
            this.clientState = clientState;
        }

        private T SaveInMemory;

        public T GetCharacterSaveInMemory()
        {
            if (SaveInMemory == null)
            {
                return LoadCharacterSave();
            }

            return SaveInMemory;
        }

        public T LoadCharacterSave()
        {
            T save = LoadSave(true);
            SaveInMemory = save;
            return save;
        }

        public void WriteCharacterSave()
        {
            logService.Debug("Writing save state for current character");
            var gameState = GetCharacterSaveInMemory();
            WriteSave(gameState, true);
        }

        public void WriteCharacterSave(T gameState)
        {
            logService.Debug("Writing save state for current character");
            WriteSave(gameState, true);
        }

        public T LoadSave(bool characterDepenent = false)
        {
            return LoadObjectFromFile<T>(characterDepenent);
        }

        public void WriteSave(T gameState, bool characterDepenent = false)
        {
            try
            {
                LastTimeSaved = DateTime.Now;
                SaveObjectToFile(gameState, characterDepenent);
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Error on game save.");
            }
        }


        private T LoadObjectFromFile<T>(bool characterDependent = false) where T : new()
        {
            var path = characterDependent ? GetCharacterRoute() : saveFileRoute;
            if (!File.Exists(path))
            {
                return new T();
            }

            string jsonText = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
            };
            T result = JsonSerializer.Deserialize<T>(jsonText, options) ?? throw new Exception($"Error loading file {path}.");

            return result;
        }

        private void SaveObjectToFile<T>(T obj, bool characterDependent = false)
        {
            try
            {
                string jsonText = JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                });

                var path = characterDependent ? GetCharacterRoute() : saveFileRoute;

                File.WriteAllText(path, jsonText);
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Error when trying to save game state");
            }
        }

        private string GetCharacterRoute()
        {
            var charFullName = clientState.LocalPlayer?.GetFullName() ?? string.Empty;
            int lastDotIndex = saveFileRoute.LastIndexOf(".");
            string pathWithoutExtension = saveFileRoute.Substring(0, lastDotIndex);
            string characterPath = $"{pathWithoutExtension}{charFullName}{Path.GetExtension(saveFileRoute)}";

            return characterPath;
        }
    }
}
