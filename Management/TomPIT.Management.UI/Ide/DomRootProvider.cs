using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Management.Dom;

namespace TomPIT.Management.Ide
{
	internal static class DomRootProvider
	{
		public static List<IDomElement> QueryDomRoot(IEnvironment environment)
		{
			var r = new List<IDomElement>
			{
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
