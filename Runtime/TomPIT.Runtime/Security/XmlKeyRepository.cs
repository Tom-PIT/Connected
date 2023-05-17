using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TomPIT.Security
{
   public class XmlKeyRepository : IXmlRepository
   {
      protected IXmlKeyService Service => Tenant.GetService<IXmlKeyService>();

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
