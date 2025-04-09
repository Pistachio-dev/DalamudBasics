using Dalamud.Plugin;
using DalamudBasics.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DalamudBasics.Configuration
{
    internal class ConfigurationService<T> : IConfigurationService<T> where T : IConfiguration, new()
    {
        private readonly ILogService logService;
        private string fileRoute;
        private T? configuration;

        public ConfigurationService(IDalamudPluginInterface pluginInterface, ILogService logService)
        {
            string configDirectory = pluginInterface?.GetPluginConfigDirectory() ?? throw new Exception("Could not retrieve the configuration directory route.");
            this.fileRoute = configDirectory + ".json";
            this.logService = logService;
        }

        public T GetConfiguration()
        {
            if (configuration != null)
            {
                return configuration;
            }

            if (!File.Exists(fileRoute))
            {
                configuration = new T();
                SaveConfiguration();
                return configuration;
            }

            string jsonText = File.ReadAllText(fileRoute);
            configuration = JsonConvert.DeserializeObject<T>(jsonText) ?? throw new Exception("Error loading configuration, loaded result is null.");

            return configuration;
        }

        public void SaveConfiguration()
        {
            string jsonText = JsonConvert.SerializeObject(GetConfiguration());
            File.WriteAllText(fileRoute, jsonText);
            logService.Info($"{nameof(T)} saved.");
        }
    }
}
