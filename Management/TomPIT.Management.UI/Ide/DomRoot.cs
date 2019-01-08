using System.Collections.Generic;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	internal static class DomRoot
	{
		public static List<IDomElement> QueryDomRoot(IEnvironment environment)
		{
			var r = new List<IDomElement>
			{
				new ResourceGroupsElement(environment),
				new SecurityElement(environment),
				new EnvironmentElement(environment),
			};

			return r;
		}
	}
}
