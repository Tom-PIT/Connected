using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Subscription : SourceCodeConfiguration, ISubscription
	{
		private IServerEvent _subscribe = null;
		private IServerEvent _subscribed = null;
		private ListItems<ISubscriptionEvent> _events = null;

		[EventArguments(typeof(SubscriptionSubscribeArguments))]
		public IServerEvent Subscribe
		{
			get
			{
				if (_subscribe == null)
					_subscribe = new ServerEvent { Parent = this };

				return _subscribe;
			}
		}

		[EventArguments(typeof(SubscriptionSubscribedArguments))]
		public IServerEvent Subscribed
		{
			get
			{
				if (_subscribed == null)
					_subscribed = new ServerEvent { Parent = this };

				return _subscribed;
			}
		}

		[Items("TomPIT.Application.Design.Items.SubscriptionEventsCollection, TomPIT.Application.Design")]
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
