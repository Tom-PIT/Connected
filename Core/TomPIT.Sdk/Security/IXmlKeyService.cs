using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TomPIT.Security
{
    public interface IXmlKeyService
    {
        ImmutableList<XElement> Query();
        XElement Select(string namespaceName);
        void Upsert(XElement element, string friendlyName);
    }
}
