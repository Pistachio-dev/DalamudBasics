using System;

namespace DalamudBasics.Logging
{
    public interface ILogService
    {
        void AttachToGameLogicLoop();

        void Debug(string message);

        void Error(Exception ex, string message);

        void Error(string message);

        void Fatal(Exception ex, string message);

        void Info(string message);

        void Warning(string message);
    }
}
