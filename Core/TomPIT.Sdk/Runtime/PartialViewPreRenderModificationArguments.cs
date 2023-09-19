using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Runtime
{
    public class PartialViewPreRenderModificationArguments
    {
        public JObject Arguments { get; set; }

        public IMicroService MicroService { get; set;  }

        public IPartialViewConfiguration Configuration { get; set; }

        public IComponent Component { get; set; }

        public string Name { get; set; }
    }
}
