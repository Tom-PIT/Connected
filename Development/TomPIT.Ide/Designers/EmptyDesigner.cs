using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public class EmptyDesigner : DomDesigner<DomElement>
	{
		public EmptyDesigner(IEnvironment environment, DomElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Empty.cshtml";
	}
}
