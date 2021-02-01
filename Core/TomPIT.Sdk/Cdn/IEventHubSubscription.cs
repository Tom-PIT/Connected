using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn
{
	public interface IEventHubSubscription
	{
		string Name { get; }
		JObject Authorization { get; }
		JObject Arguments { get; }
	}
}
