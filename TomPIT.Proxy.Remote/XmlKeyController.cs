using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace TomPIT.Proxy.Remote
{
	internal class XmlKeyController : IXmlKeyController
	{
		private const string Controller = "XmlKey";
		public ImmutableList<XElement> Query()
		{
			return Connection.Get<List<XElement>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList();
		}

		public XElement Select(string namespaceName)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("id", namespaceName);

			return Connection.Get<XElement>(u);
		}

		public void Upsert(object element, string id, string friendlyName)
		{
			Connection.Post<string>(Connection.CreateUrl(Controller, "Upsert"), new
			{
				id,
				element,
				friendlyName
			});
		}
	}
}
