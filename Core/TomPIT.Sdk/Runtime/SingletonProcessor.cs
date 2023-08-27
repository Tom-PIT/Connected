using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Runtime
{
    public class SingletonProcessor<T>
    {
        private Lazy<ConcurrentDictionary<T, ManualResetEventSlim>> _state = new Lazy<ConcurrentDictionary<T, ManualResetEventSlim>>();
        private bool _isInitialized = false;
        //private HashSet<string> _threadState = new HashSet<string>();

        public void Start(T value, Action create, Action retrieve)
        {
            var resetEvent = new ManualResetEventSlim(false);

            if (!State.TryAdd(value, resetEvent))
            {
                resetEvent.Dispose();

                //if(_threadState.Contains($"{value}.{Thread.CurrentThread.ManagedThreadId}"))
                //{
                //	throw new Exception("Cycle detected.");
                //}

                if (State.TryGetValue(value, out resetEvent))
                {
                    try
                    {
                        resetEvent.Wait();
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }

                retrieve();
            }
            else
            {
                try
                {
                    //if(!_threadState.Add($"{value}.{Thread.CurrentThread.ManagedThreadId}"))
                    //{
                    //	throw new Exception("Cycle?");
                    //}

                    _isInitialized = true;
                    create();
                }
                finally
                {
                    resetEvent.Set();
                    //_threadState.Remove($"{value}.{Thread.CurrentThread.ManagedThreadId}");

                    if (State.TryRemove(value, out ManualResetEventSlim e))
                        e.Dispose();
                }
            }
        }

        public void Start(T value, Action create)
        {
            var resetEvent = new ManualResetEventSlim(false);

            if (!State.TryAdd(value, resetEvent))
            {
                resetEvent.Dispose();

                if (State.TryGetValue(value, out resetEvent))
                {
                    try
                    {
                        resetEvent.Wait();
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }
            }
            else
            {
                try
                {
                    _isInitialized = true;
                    create();
                }
                finally
                {
                    resetEvent.Set();

                    if (State.TryRemove(value, out ManualResetEventSlim e))
                        e.Dispose();
                }
            }
        }

        public async Task Start(T value, Func<Task> create)
        {
            var resetEvent = new ManualResetEventSlim(false);

            if (!State.TryAdd(value, resetEvent))
            {
                resetEvent.Dispose();

                if (State.TryGetValue(value, out resetEvent))
                {
                    try
                    {
                        resetEvent.Wait();
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }
            }
            else
            {
                try
                {
                    _isInitialized = true;
                    await create();
                }
                finally
                {
                    resetEvent.Set();

                    if (State.TryRemove(value, out ManualResetEventSlim e))
                        e.Dispose();
                }
            }
        }

        private ConcurrentDictionary<T, ManualResetEventSlim> State => _state.Value;

        public bool IsInitialized => _isInitialized;
    }
}
