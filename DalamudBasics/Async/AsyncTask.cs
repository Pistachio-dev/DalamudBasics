using System;
using System.Threading.Tasks;

namespace DalamudBasics.Async
{
    public class AsyncTask
    {
        public AsyncTask(string identifier, Task task, Action<Exception?>? onFailure = null)
        {
            Identifier = identifier;
            Task = task;
            OnFailure = onFailure;
        }

        public bool IsCompleted => Task.IsCompleted;
        public bool IsFaulted => Task.IsFaulted;
        public Exception? Exception => Task.Exception;

        public void OnCompletion()
        {
            if (Task.Status == TaskStatus.Faulted)
            {
                if (OnFailure != null)
                {
                    OnFailure(Task.Exception);
                    return;
                }
            }
        }

        public string Identifier { get; set; }
        public Task Task { get; set; }
        public Action<Exception?>? OnFailure { get; set; }
    }
}
