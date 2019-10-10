using Newtonsoft.Json;

namespace TomPIT.Cdn.Data
{
	[JsonConverter(typeof(DataHubEndpointPolicySubscriberConverter))]
	public class DataHubEndpointPolicySubscriber : IDataHubEndpointPolicySubscriber
	{
		public string Name { get; set; }
		public string Arguments { get; set; }
	}
}
