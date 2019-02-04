using NLog;
using System;
using System.Threading;

namespace ICM.Common.Multithreading
{
    /// <summary>
    /// Used to await all worker threads exit, could be used globally
    /// </summary>
    public class WorkerCountdown : IDisposable
    {
        private CountdownEvent _event;
        //private Logger Log = LogManager.GetCurrentClassLogger();

        public int CurrentCount => _event.CurrentCount;

        public WorkerCountdown()
        {
            _event = new CountdownEvent(1);
        }

        public void AddCount()
        {
            _event.AddCount();
            //Log.Log(LogLevel.Debug, $"Workers after Add: {CurrentCount}");
        }

        public void AddCount(int signalCount)
        {
            _event.AddCount(signalCount);
            //Log.Log(LogLevel.Debug, $"Workers after Add: {CurrentCount}");
        }

        public void Signal()
        {
            _event.Signal();
            //Log.Log(LogLevel.Debug, $"Workers after Signal: {CurrentCount}");
        }

        public void Signal(int signalCount)
        {
            _event.Signal(signalCount);
            //Log.Log(LogLevel.Debug, $"Workers after Signal: {CurrentCount}");
        }

        public void Wait(int milliseconds)
        {
            _event.Wait(milliseconds);
        }

        public void Wait()
        {
            _event.Wait();
        }

        public void Dispose()
        {
            _event.Dispose();
        }
    }
}
