using DalamudBasics.Logging;
using System;
using System.Collections.Generic;

namespace DalamudBasics.Async
{
    internal class AsyncTaskManager : IAsyncTaskManager
    {
        private Dictionary<string, AsyncTask> runningTasks = new();
        private readonly ILogService log;

        public AsyncTaskManager(ILogService log)
        {
            this.log = log;
        }

        public void RunTask(AsyncTask task)
        {
            if (runningTasks.ContainsKey(task.Identifier))
            {
                log.Error($"Attempting to run a task whose identifier {task.Identifier} already exists");
                return;
            }

            runningTasks.Add(task.Identifier, task);
            log.Info($"Running task with identifier {task.Identifier}.");
        }

        public bool IsTaskRunning(string identifier)
        {
            if (!runningTasks.ContainsKey(identifier))
            {
                return false;
            }

            var task = runningTasks[identifier];
            if (task.IsCompleted)
            {
                task.OnCompletion();

                LogResult(task);
                runningTasks.Remove(identifier);

                return false;
            }

            return true;
        }

        private void LogResult(AsyncTask task)
        {
            if (task.IsFaulted)
            {
                log.Error(task.Exception ?? new Exception($"No exception for faulted task."), $"Task with identifier {task.Identifier} has finished with an error.");
            }
            else
            {
                log.Info($"Task with identifier {task.Identifier} has finished successfully.");
            }
        }
    }
}
