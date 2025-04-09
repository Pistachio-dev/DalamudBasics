namespace DalamudBasics.Async
{
    internal interface IAsyncTaskManager
    {
        bool IsTaskRunning(string identifier);

        void RunTask(AsyncTask task);
    }
}
