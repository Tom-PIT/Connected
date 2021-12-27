using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
    public interface IXmlKeyHandler
    {
        List<IXmlKey> Query();
        IXmlKey Select(string id);
        void Upsert(string id, string friendlyName, string encryptedElement);
    }
}
