using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients
{
    public static class Extensions
    {
        public static void TryDispose(this IDisposable f)
        {
            try { f?.Dispose(); } catch { }
        }
    }
}
