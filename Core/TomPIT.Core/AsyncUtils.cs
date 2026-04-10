using System;
using System.Threading.Tasks;

namespace TomPIT
{
    public static class AsyncUtils
    {
        public static void RunSync(Func<Task> func)
        {
            func().GetAwaiter().GetResult();
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return func().GetAwaiter().GetResult();
        }
    }
}
