using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.App.Resources;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.UI;

namespace TomPIT.App.Resources
{
    internal class ViewArgumentService : ClientRepository<ViewArgumentsCacheEntry, Guid>, IViewArgumentService
    {
        private readonly object _lock = new();

        public ViewArgumentService(ITenant tenant) : base(tenant, "viewArguments")
        {
        }

        public Guid Insert(ViewArgumentsCacheEntry viewArguments)
        {
            lock (_lock)
            {
                var key = Guid.NewGuid();

                Set(key, viewArguments, TimeSpan.FromSeconds(30));

                return key;
            }
        }

        public JObject Select(Guid key)
        {
            lock (_lock)
            {
                var entry = Get(key);

                if (entry is null)
                    return null;

                entry.InjectionCount--;

                if (entry.InjectionCount == 1)
                    Remove(key);
                else
                    Set(key, entry);

                return entry.Arguments;
            }
        }
    }
}
