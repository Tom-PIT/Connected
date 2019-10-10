using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Cdn.Data
{
	public class DataHubEndpointSubscriber : IDataHubEndpointSubscriber
	{
		private List<IDataHubEndpointPolicySubscriber> _policies = null;
		public string Name { get; set; }

		[JsonConverter(typeof(DataHubEndpointPoliciesSubscriberConverter))]
		public List<IDataHubEndpointPolicySubscriber> Policies
		{
			get
			{
				if (_policies == null)
					_policies = new List<IDataHubEndpointPolicySubscriber>();

				return _policies;
			}
		}
	}
}
