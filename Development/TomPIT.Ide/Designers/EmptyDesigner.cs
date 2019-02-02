using TomPIT.Dom;

namespace TomPIT.Designers
{
	public class EmptyDesigner : DomDesigner<DomElement>
	{
		public EmptyDesigner(DomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Empty.cshtml";
	}
}
