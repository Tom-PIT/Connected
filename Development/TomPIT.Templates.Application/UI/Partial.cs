using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Partial")]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("razor")]
	public class Partial : ViewBase, IPartialView
	{
		public const string ComponentCategory = "Partial";
	}
}
