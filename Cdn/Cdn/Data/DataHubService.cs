using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Data
{
	internal class DataHubService : TenantObject, IDataHubService
	{
		public DataHubService(ITenant tenant) : base(tenant)
		{
		}

		public async Task NotifyAsync(DataHubNotificationArgs e)
		{
			var args = new JObject
			{
				{"event", e.Name }
			};

			if (!string.IsNullOrWhiteSpace(e.Arguments))
				args.Add("arguments", Serializer.Deserialize<JObject>(e.Arguments));

			await DataHubs.Data.Clients.Group(e.Name.ToLowerInvariant()).SendCoreAsync("data", new object[] { args });
		}
	}
}
