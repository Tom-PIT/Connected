using TomPIT.Annotations;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Master")]
	[DomDesigner("TomPIT.Design.TemplateDesigner, TomPIT.Ide")]
	public class MasterView : ViewBase, IMasterView
	{
		public const string ComponentCategory = "MasterView";
	}
}
