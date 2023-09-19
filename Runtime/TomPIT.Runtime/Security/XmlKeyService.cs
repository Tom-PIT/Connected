using System;
using System.Collections.Immutable;
using System.Xml.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	public class XmlKeyService : SynchronizedClientRepository<XElement, string>, IXmlKeyService, IXmlKeyNotification
	{
		public XmlKeyService(ITenant tenant) : base(tenant, "XmlKeyRepository") { }

		protected override void OnInitializing()
		{
			var elements = Instance.SysProxy.XmlKeys.Query();

			foreach (var element in elements)
				Set(element.Attribute("id").Value, element, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var r = Instance.SysProxy.XmlKeys.Select(id);

			if (r is not null)
				Set(r.Attribute("id").Value, r, TimeSpan.Zero);
		}

		public void NotifyChanged(object sender, XmlKeyEventArgs e)
		{
			Refresh(e.Id);
		}

		public ImmutableList<XElement> Query()
		{
			return All();
		}

		public void Upsert(XElement element, string friendlyName)
		{
			var id = element.Attribute("id").Value;

			Instance.SysProxy.XmlKeys.Upsert(element, id, friendlyName);

			if (Tenant.GetService<IXmlKeyService>() is IXmlKeyNotification n)
				n.NotifyChanged(this, new XmlKeyEventArgs(id));
		}

		public XElement Select(string id)
		{
			return Get(id);
		}
	}
}
