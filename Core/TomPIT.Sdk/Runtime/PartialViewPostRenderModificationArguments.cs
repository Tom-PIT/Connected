using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.ComponentModel;

namespace TomPIT.Runtime
{
    public class PartialViewPostRenderModificationArguments : PartialViewPreRenderModificationArguments
    {
        public string Content { get; set; }
    }
}
