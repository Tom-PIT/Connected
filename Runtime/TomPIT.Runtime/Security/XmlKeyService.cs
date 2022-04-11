using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
    public class XmlKeyService : SynchronizedClientRepository<XElement, string>, IXmlKeyService, IXmlKeyNotification
    {
        public XmlKeyService(ITenant tenant) : base(tenant, "XmlKeyRepository") { }

        protected override void OnInitializing()
        {
            var u = Tenant.CreateUrl("XmlKey", "Query");
            var elements = Tenant.Get<List<XElement>>(u).ToImmutableList();

            foreach (var element in elements)
            {
                Set(element.Attribute("id").Value, element, TimeSpan.Zero);
            }
        }
        protected override void OnInvalidate(string id)
        {
            var u = Tenant.CreateUrl("XmlKey", "Select").AddParameter("id", id);

            var r = Tenant.Get<XElement>(u);

            if (r != null)
                Set(r.Attribute("id").Value, r, TimeSpan.Zero);
        }

        public void NotifyChanged(object sender, XmlKeyEventArgs e)
        {
            Refresh(e.Id);
        }

        public ImmutableList<XElement> Query()
        {
            var values = All();
            return values;
        }

        public void Upsert(XElement element, string friendlyName)
        {
            var id = element.Attribute("id").Value;

            var u = Tenant.CreateUrl("XmlKey", "Upsert");
            var e = new JObject
            {
                { "id", id },
                { "element", JToken.FromObject(element) },
                { "friendlyName", friendlyName}
            };

            Tenant.Post<string>(u, e);

            if (Tenant.GetService<IXmlKeyService>() is IXmlKeyNotification n)
                n.NotifyChanged(this, new XmlKeyEventArgs(id));
        }

        public XElement Select(string id)
        {
            return Get(id);
        }
    }
}
