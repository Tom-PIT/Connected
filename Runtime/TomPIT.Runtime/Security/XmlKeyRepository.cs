using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
    public class XmlKeyRepository : IXmlRepository
    {
        protected IXmlKeyService Service => MiddlewareDescriptor.Current.Tenant.GetService<IXmlKeyService>();

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return Service.Query();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            Service.Upsert(element, friendlyName);
        }
    }
}
