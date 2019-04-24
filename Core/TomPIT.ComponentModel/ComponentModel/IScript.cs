using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.ComponentModel
{
    public interface IScript : IConfiguration, ISourceCode
    {
        ElementScope Scope { get; }
    }
}
