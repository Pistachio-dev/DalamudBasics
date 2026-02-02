using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudBasics.Extensions;
using DalamudBasics.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DalamudBasics.SaveGames
{
    public class SaveManager<T> : IDisposable, ISaveManager<T> where T : new()
    {
        private readonly string saveFileRoute;
        private readonly ILogService logService;
        private readonly IObjectTable objectTable;
        private readonly IClientState clientState;
        private readonly IFramework framework;
        private DateTime? lastTimeSaved;

        private T? saveInMemory;
        private string? lastCharacterLoaded;

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

        public SaveManager(string fileName, ILogService logService, IClientState clientState, IFramework framework, IDalamudPluginInterface pi, IObjectTable objectTable)
        {
            this.saveFileRoute = pi.GetPluginConfigDirectory() + Path.DirectorySeparatorChar + fileName;
            this.logService = logService;
            this.clientState = clientState;
            this.framework = framework;
            clientState.Login += LoadSaveOnLogin;
            this.objectTable = objectTable;
        }

        public T? GetCharacterSaveInMemory()
        {
            if (!IsLocalCharacterAvailable())
            {
                return default;
            }
            var charName = GetCurrentCharFullName();

            if (saveInMemory == null || charName != lastCharacterLoaded)
            {
                logService.Info($"Loading save for character {GetCurrentCharFullName()}");
                return LoadCharacterSave(charName);
            }

            return saveInMemory;
        }

        public void WriteCharacterSave()
        {
            if (!IsLocalCharacterAvailable())
            {
                logService.Debug("Write save cancelled: no local character available");
                return;
            }
            logService.Debug("Writing save state for current character");
            var gameState = GetCharacterSaveInMemory();
            if (gameState == null)
            {
                logService.Debug("Game state is null. Skipping save.");
                return;
            }
            WriteSave(gameState, true);
        }

        private void LoadSaveOnLogin()
        {
            framework.RunOnFrameworkThread(() =>
            {
                GetCharacterSaveInMemory();
            });
        }

        private T LoadCharacterSave(string charName)
        {
            T save = LoadSave(true);
            lastCharacterLoaded = charName;
            saveInMemory = save;
            return save;
        }

        private T LoadSave(bool characterDependent = false)
        {
            return LoadObjectFromFile<T>(characterDependent);
        }

        private void WriteSave(T gameState, bool characterDepenent = false)
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

        private K LoadObjectFromFile<K>(bool characterDependent = false) where K : new()
        {
            var path = characterDependent ? GetCharacterRoute() : saveFileRoute;
            if (!File.Exists(path))
            {
                return new K();
            }

            string jsonText = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
            };
            K result = JsonSerializer.Deserialize<K>(jsonText, options) ?? throw new Exception($"Error loading file {path}.");

            return result;
        }

        private void SaveObjectToFile<K>(K obj, bool characterDependent = false)
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
            var charFullName = GetCurrentCharFullName();
            int lastDotIndex = saveFileRoute.LastIndexOf(".");
            string pathWithoutExtension = saveFileRoute.Substring(0, lastDotIndex);
            string characterPath = $"{pathWithoutExtension}{charFullName}{Path.GetExtension(saveFileRoute)}";

            return characterPath;
        }

        private string GetCurrentCharFullName()
        {
            return objectTable.LocalPlayer?.GetFullName() ?? string.Empty;
        }

        private bool IsLocalCharacterAvailable()
        {
            return objectTable.LocalPlayer != null;
        }

        public void Dispose()
        {
            clientState.Login -= LoadSaveOnLogin;
        }
    }
}
