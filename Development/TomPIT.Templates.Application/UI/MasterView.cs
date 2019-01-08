using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Master")]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("razor")]
	public class MasterView : ViewBase, IMasterView
	{
		public const string ComponentCategory = "MasterView";
	}
}
