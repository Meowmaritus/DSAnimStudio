using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SafeSingleThreadDispatcher : IDisposable
    {
        private readonly object _lock_Invoke = new object();
        private readonly object _lock_Params = new object();

        private AutoResetEvent event_DoAction = new AutoResetEvent(false);
        private AutoResetEvent event_ActionFinished = new AutoResetEvent(false);

        private bool workerLoopRunning = false;

        private bool stopRequested = false;




        private Action nextAction = null;

        private Thread thread;
        public SafeSingleThreadDispatcher()
        {
            thread = new Thread(ThreadProc);
            thread.Start();
        }

        public void RequestStop()
        {
            lock (_lock_Invoke)
            {
                bool _stopRequested = false;
                lock (_lock_Params)
                    _stopRequested = stopRequested;
                if (!_stopRequested)
                {


                    lock (_lock_Params)
                    {
                        stopRequested = true;
                        nextAction = null;
                    }

                    event_DoAction.Set();
                    event_ActionFinished.WaitOne();
                }
            }
        }

        public void Invoke(Action action)
        {
            lock (_lock_Invoke)
            {
                bool _stopRequested = false;
                lock (_lock_Params)
                    _stopRequested = stopRequested;
                if (!_stopRequested)
                {
                    lock (_lock_Params)
                    {
                        nextAction = action;
                    }

                    event_DoAction.Set();
                    event_ActionFinished.WaitOne();
                }
            }
            
        }

        private void ThreadProc()
        {

            workerLoopRunning = true;
            while (workerLoopRunning)
            {
                event_DoAction.WaitOne();

                bool _stopRequested = false;
                lock (_lock_Params)
                    _stopRequested = stopRequested;

                if (_stopRequested)
                {
                    workerLoopRunning = false;
                    event_ActionFinished.Set();
                }
                else
                {
                    lock (_lock_Params)
                    {
                        if (nextAction != null)
                        {
                            nextAction();
                            nextAction = null;
                        }
                        else
                        {
                            throw new Exception("?????");
                        }
                    }

                    event_ActionFinished.Set();

                }


                
            }

            
        }

        public void Dispose()
        {
            bool _stopRequested = false;
            lock (_lock_Params)
                _stopRequested = stopRequested;

            if (!_stopRequested)
            {
                RequestStop();
                event_DoAction?.Dispose();
                event_DoAction = null;
                event_ActionFinished?.Dispose();
                event_ActionFinished = null;
            }
        }
    }
}
