using System.Collections.Generic;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	internal class Dom : DomBase
	{
		public Dom(IEnvironment environment, string path) : base(environment, path)
		{
			Initialize();
		}

		protected override List<IDomElement> Root()
		{
			return DomRoot.QueryDomRoot(Environment);
		}
	}
}