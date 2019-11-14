using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Messaging
{
	public class EventBindings : SourceCodeConfiguration, IEventBindingConfiguration
	{
		private ListItems<IEventBinding> _bindings = null;

		[Items(DesignUtils.EventBindingsItems)]
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
