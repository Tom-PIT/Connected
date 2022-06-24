using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TomPIT.Security
{
    public interface IXmlKey
    {   string Id { get; }
        string FriendlyName { get; }

        XElement Element { get; }
    }
}
