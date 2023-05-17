using System.Collections.Immutable;
using System.Xml.Linq;

namespace TomPIT.Proxy
{
	public interface IXmlKeyController
	{
		ImmutableList<XElement> Query();
		XElement Select(string namespaceName);
		void Upsert(object element, string id, string friendlyName);
	}
}
