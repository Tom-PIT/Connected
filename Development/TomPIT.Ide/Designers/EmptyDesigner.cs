using TomPIT.Ide.Dom;

namespace TomPIT.Ide.Designers
{
	public class EmptyDesigner : DomDesigner<DomElement>
	{
		public EmptyDesigner(DomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Empty.cshtml";
	}
}
