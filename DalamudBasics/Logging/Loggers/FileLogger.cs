using Dalamud.Plugin;
using System.IO;

namespace DalamudBasics.Logging.Loggers
{
    internal class FileLogger : IFileLogger
    {
        private readonly string fileRoute;

        public FileLogger(IDalamudPluginInterface pluginInterface)
        {
            this.fileRoute = pluginInterface.GetPluginConfigDirectory() + Path.DirectorySeparatorChar + "logs.txt";
        }

        public void Log(string message)
        {
            using (StreamWriter sw = File.AppendText(fileRoute))
            {
                sw.WriteLine(message);
            }
        }
    }
}
