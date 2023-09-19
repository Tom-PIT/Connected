using System.Collections.Immutable;
using System.Xml.Linq;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class XmlKeyController : IXmlKeyController
	{
		public ImmutableList<XElement> Query()
		{
			return DataModel.XmlKeys.Query();
		}

		public XElement Select(string namespaceName)
		{
			return DataModel.XmlKeys.Select(namespaceName);
		}

		public void Upsert(object element, string id, string friendlyName)
		{
			DataModel.XmlKeys.Upsert(id, friendlyName, element as XElement);
		}
	}
}
