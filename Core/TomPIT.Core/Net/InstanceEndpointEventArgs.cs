using System;

namespace TomPIT.Net
{
	public class InstanceEndpointEventArgs : EventArgs
	{
		public InstanceEndpointEventArgs(Guid endpoint)
		{
			Endpoint = endpoint;
		}

		public Guid Endpoint { get; }
	}
}
