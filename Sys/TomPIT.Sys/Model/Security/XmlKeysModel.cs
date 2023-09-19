using System;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Linq;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Security
{
   public class XmlKeysModel : SynchronizedRepository<XElement, string>
   {
      public XmlKeysModel(IMemoryCache container) : base(container, "xmlKey")
      {
      }

      protected override void OnInitializing()
      {
         var ds = Shell.GetService<IDatabaseService>().Proxy.Security.XmlKeys.Query();

         foreach (var i in ds)
            Set(i.Id.ToLowerInvariant(), i.Element, TimeSpan.Zero);
      }

      protected override void OnInvalidate(string id)
      {
         var r = Shell.GetService<IDatabaseService>().Proxy.Security.XmlKeys.Select(id);

         if (r is null)
         {
            Remove(id);
            return;
         }

         Set(id.ToLowerInvariant(), r.Element, TimeSpan.Zero);
      }

      public ImmutableList<XElement> Query()
      {
         return All();
      }

      public XElement Select(string id)
      {
         return Get(id);
      }

      public void Upsert(string id, string friendlyName, XElement element)
      {
         using var ms = new MemoryStream();

         element.Save(ms, SaveOptions.DisableFormatting);
         ms.Position = 0;

         using var sr = new StreamReader(ms);

         Shell.GetService<IDatabaseService>().Proxy.Security.XmlKeys.Upsert(id, friendlyName, sr.ReadToEnd());
         Refresh(id);
         CachingNotifications.XmlKeyChanged(id);
      }
   }
}
