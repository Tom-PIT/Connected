using System.Collections.Generic;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;
using TomPIT.Management.Dom;

namespace TomPIT.Management.Ide
{
	internal static class DomRootProvider
	{
		public static List<IDomElement> QueryDomRoot(IEnvironment environment)
		{
			var r = new List<IDomElement>
			{
				new MarketplaceElement(environment),
				new IntegrationElement(environment),
				new ResourceGroupsElement(environment),
				new SecurityElement(environment),
				new GlobalizationElement(environment),
				new EnvironmentElement(environment),
				new BigDataElement(environment)
			};


			return r;
		}
	}
}
