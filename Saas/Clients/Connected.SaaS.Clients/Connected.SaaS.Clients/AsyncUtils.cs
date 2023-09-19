using System;
using System.Threading;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients
{
    public static class AsyncUtils
    {
        private static readonly TaskFactory _factory = new
                     (CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static void RunSync(Func<Task> func)
        {
            _factory
               .StartNew(func)
               .Unwrap()
               .GetAwaiter()
               .GetResult();
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _factory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
