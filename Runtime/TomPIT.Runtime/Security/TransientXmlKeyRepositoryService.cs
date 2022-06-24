using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
    public class TransientXmlKeyRepositoryService : SynchronizedClientRepository<XElement, string>, IXmlKeyService, IXmlKeyNotification
    {
        private ConcurrentDictionary<string, XElement> _storage = new ConcurrentDictionary<string, XElement>();

        public TransientXmlKeyRepositoryService(ITenant tenant) : base(tenant, "XmlKeyRepository") { }

        protected override void OnInvalidate(string id)
        {
            var r = Select(id);

            if (r != null)
                Set(id, r, TimeSpan.Zero);
        }

        public void NotifyChanged(object sender, XmlKeyEventArgs e)
        {
            Refresh(e.Id);
        }

        public ImmutableList<XElement> Query()
        {
            return _storage.Values.ToImmutableList();
        }

        public void Upsert(XElement element, string friendlyName)
        {
            _storage.AddOrUpdate(element.Attribute("id").Value, (key) => element, (key, existing) => element);
        }

        public XElement Select(string id)
        {
            return _storage.TryGetValue(id, out var tmp) ? tmp : null;
        }
    }
}
