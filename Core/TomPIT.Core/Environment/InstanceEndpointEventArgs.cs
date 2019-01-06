using System;

namespace TomPIT.Environment
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
