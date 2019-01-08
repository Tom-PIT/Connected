using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Events
{
	[DefaultEvent(nameof(Invoke))]
	public class EventHandler : ComponentConfiguration, IEventHandler
	{
		private IServerEvent _invoke = null;
		private ListItems<IEventBinding> _bindings = null;
		public const string ComponentCategory = "EventHandler";

		private ListItems<IText> _scripts = null;

		[Items("TomPIT.Application.Design.Items.LibraryScriptCollection, TomPIT.Application.Design")]
		public ListItems<IText> Scripts
		{
			get
			{
				if (_scripts == null)
					_scripts = new ListItems<IText> { Parent = this };

				return _scripts;
			}
		}

		[EventArguments(typeof(EventInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}

		[Items("TomPIT.Application.Design.Items.EventBindingsCollection, TomPIT.Application.Design")]
		public ListItems<IEventBinding> Events
		{
			get
			{
				if (_bindings == null)
					_bindings = new ListItems<IEventBinding> { Parent = this };

				return _bindings;
			}
		}
	}
}
