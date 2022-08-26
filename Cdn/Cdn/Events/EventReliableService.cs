using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Events
{
	internal  class EventReliableService: HostedService
	{
		public EventReliableService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override async Task OnExecute(CancellationToken cancel)
		{
			var items = EventMessagingCache.Dequeue();

			foreach (var item in items)
			{
				var message = new
				{
					MessageId = item.Id,
					item.Recipient
				};

				await EventHubs.Events.Clients.Client(item.Connection).SendCoreAsync("event", new object[] { item.Arguments, message }, cancel);
			}
		}
	}
}
