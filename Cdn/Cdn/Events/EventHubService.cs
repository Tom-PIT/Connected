using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Events
{
	internal class EventHubService : TenantObject, IEventHubService
	{
		public EventHubService(ITenant tenant) : base(tenant)
		{
		}

		public async Task NotifyAsync(EventHubNotificationArgs e)
		{
			var args = new JObject
			{
				{"event", e.Name }
			};

			if (e.Arguments != null)
				args.Add("arguments", Serializer.Deserialize<JObject>(e.Arguments));

			if (EventHubs.Events != null)
				await EventHubs.Events.Clients.Group(e.Name.ToLowerInvariant()).SendCoreAsync("event", new object[] { args });
		}
	}
}
