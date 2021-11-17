using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.UI
{
    public class ViewArgumentsCacheEntry
    {
        public JObject Arguments { get; set; }
        public int InjectionCount { get; set; }
    }
}
