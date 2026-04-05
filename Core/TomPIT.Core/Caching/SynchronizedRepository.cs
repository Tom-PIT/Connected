using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;

namespace TomPIT.Caching
{
    public abstract class SynchronizedRepository<T, K> : CacheRepository<T, K>, IDisposable where T : class
    {
        private bool _disposed = false;
        protected object SyncRoot = new object();
        public event CacheInvalidateHandler Invalidate;

        protected SynchronizedRepository(IMemoryCache container, string key) : base(container, key)
        {
            /*
			    * Hook to the second event in the invalidation process
			    * - Invalidating
			    * - Invalidate
			    * - Invalidated
			    */
            Container.Invalidate += OnInvalidate;
        }

        protected virtual InvalidateBehavior InvalidateBehavior { get; } = InvalidateBehavior.KeepSameInstance;

        private void OnInvalidate(CacheEventArgs e)
        {
            if (string.Compare(e.Key, Key, false) == 0)
            {
                if (Initialized)
                {
                    var id = Types.Convert<K>(e.Id, CultureInfo.InvariantCulture);
                    var existedBefore = Get(id) is not null;

                    OnInvalidate(id);

                    //Add resiliance against database transaction write lag
                    if (!existedBefore && Get(id) is null)
                    {
                        for (var i = 1; i <= 3; i++)
                        {
                            Thread.Sleep(i * 100);
                            OnInvalidate(id);

                            if (Get(id) is not null)
                                break;
                        }
                    }
                }

                Invalidate?.Invoke(e);

                e.InvalidateBehavior = InvalidateBehavior;
            }
        }

        protected virtual void OnInvalidate(K id)
        {

        }

        protected virtual void OnInitializing()
        {

        }

        protected void Initialize()
        {
            if (Initialized)
                return;

            lock (SyncRoot)
            {
                if (Initialized)
                    return;

                InitializeSignal = new ManualResetEvent(false);
                Initializing = true;

                try
                {
                    OnInitializing();
                    Initialized = true;
                }
                catch
                {

                }
                finally
                {
                    Initializing = false;
                    InitializeSignal.Set();
                    InitializeSignal.Dispose();
                    InitializeSignal = null;
                }
            }

            if (Initialized)
                OnInitialized();
        }

        protected virtual void OnInitialized()
        {

        }

        private bool Initializing { get; set; }
        private bool Initialized { get; set; }

        protected override ImmutableList<T> All()
        {
            WaitForInitialization();

            if (!Initializing)
                Initialize();

            return base.All();
        }

        protected override T First()
        {
            WaitForInitialization();

            if (!Initializing)
                Initialize();

            return base.First();
        }

        protected override T Get(Func<T, bool> predicate)
        {
            WaitForInitialization();

            if (!Initializing)
                Initialize();

            return base.Get(predicate);
        }

        protected override T Get(K id)
        {
            WaitForInitialization();

            if (!Initializing)
                Initialize();

            return base.Get(id);
        }

        protected override T Get(K id, CacheRetrieveHandler<T> retrieve)
        {
            WaitForInitialization();

            if (!Initializing)
                Initialize();

            return base.Get(id, retrieve);
        }

        protected override ImmutableList<T> Where(Func<T, bool> predicate)
        {
            WaitForInitialization();

            if (!Initializing)
                Initialize();

            return base.Where(predicate);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Container.Invalidate -= OnInvalidate;
                _disposed = true;
            }
        }

        private ManualResetEvent InitializeSignal { get; set; }

        private void WaitForInitialization()
        {
            try
            {
                InitializeSignal?.WaitOne();
            }
            catch (ObjectDisposedException)
            {
                /*
                 * Catch only the ObjectDisposedException, if waitForInitialization is called while the lock object is disposing and is not yet null. 
                 * Initialization has completed, so it is safe to continue.
                 */
            }
        }
    }
}