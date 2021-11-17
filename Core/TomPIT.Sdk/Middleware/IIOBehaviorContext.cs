using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Runtime;

namespace TomPIT.Middleware
{
    public interface IIOBehaviorContext
    {
        EnvironmentIOBehavior Behavior { get; set; }

    }
}
