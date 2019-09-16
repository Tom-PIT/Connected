using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Messaging;
using TomPIT.ComponentModel.UI;
using TomPIT.Messaging;
using TomPIT.UI;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	public class Partial : ViewBase, IPartialViewConfiguration
	{
		private IServerEvent _invoke = null;

		[EventArguments(typeof(ViewInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}
	}
}
