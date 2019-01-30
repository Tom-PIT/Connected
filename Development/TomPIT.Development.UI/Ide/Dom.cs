using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	public class Dom : DomBase
	{
		public Dom(IEnvironment environment, IMicroServiceTemplate template, string path) : base(environment, path)
		{
			Template = template;

			Initialize();
		}

		protected override List<IDomElement> Root()
		{
			return Template.QueryDomRoot(Environment, null, Environment.Context.MicroService());
		}
		private IMicroServiceTemplate Template { get; }
	}
}
