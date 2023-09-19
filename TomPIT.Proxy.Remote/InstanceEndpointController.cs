using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Environment;

namespace TomPIT.Proxy.Remote
{
	internal class InstanceEndpointController : IInstanceEndpointController
	{
		private const string Controller = "InstanceEndpoint";
		public ImmutableList<IInstanceEndpoint> Query()
		{
			return Connection.Get<List<InstanceEndpoint>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IInstanceEndpoint>();
		}

		public IInstanceEndpoint Select(Guid endpoint)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("endpoint", endpoint);

			return Connection.Get<InstanceEndpoint>(u);
		}
	}
}
