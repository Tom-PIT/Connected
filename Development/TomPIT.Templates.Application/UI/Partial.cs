using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Partial")]
	[DomDesigner("TomPIT.Design.TemplateDesigner, TomPIT.Ide")]
	public class Partial : ViewBase, IPartialView
	{
		public const string ComponentCategory = "Partial";
	}
}
