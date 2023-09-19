using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.UI
{
    public interface IViewArgumentService
    {
        Guid Insert(ViewArgumentsCacheEntry arguments);
        
        JObject Select(Guid key);
    }
}
