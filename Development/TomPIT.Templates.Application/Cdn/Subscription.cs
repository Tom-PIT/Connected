using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Subscription : SourceCodeConfiguration, ISubscriptionConfiguration
	{
		private ListItems<ISubscriptionEvent> _events = null;

		[Items(DesignUtils.SubscriptionEventsItems)]
		public ListItems<ISubscriptionEvent> Events
		{
			get
			{
				if (_events == null)
					_events = new ListItems<ISubscriptionEvent> { Parent = this };

				return _events;
			}
		}
	}
}
