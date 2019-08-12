using TomPIT.Annotations;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Partial")]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("razor")]
	public class Partial : ViewBase, IPartialView
	{
		private IServerEvent _invoke = null;

		public const string ComponentCategory = "Partial";

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
