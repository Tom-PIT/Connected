using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Ide
{
	internal class Dom : DomRoot
	{
		public Dom(IEnvironment environment, string path) : base(environment, path)
		{
			Initialize();
		}

		public override List<IDomElement> Root()
		{
			return DomRootProvider.QueryDomRoot(Environment);
		}
	}
}