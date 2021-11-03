using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.ComponentModel;

namespace TomPIT.Runtime
{
    public class ViewPreRenderModificationArguments
    {
        public JObject Arguments { get; set; }

        public IMicroService MicroService { get; set;  }

        public IConfiguration Configuration { get; set; }

        public IComponent Component { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }
    }
}
