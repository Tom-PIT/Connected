using System;
using System.Collections.Immutable;
using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class InstanceEndpointController : IInstanceEndpointController
	{
		public ImmutableList<IInstanceEndpoint> Query()
		{
			return DataModel.InstanceEndpoints.Query();
		}

		public IInstanceEndpoint Select(Guid endpoint)
		{
			return DataModel.InstanceEndpoints.GetByToken(endpoint);
		}
	}
}
