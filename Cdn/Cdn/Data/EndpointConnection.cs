using System;
using System.Collections.Concurrent;

namespace TomPIT.Cdn.Data
{
	internal class EndpointConnection
	{
		private ConcurrentDictionary<string, EndpointConnectionDescriptor> _descriptors = null;
		public string ConnectionId { get; set; }

		public ConcurrentDictionary<string, EndpointConnectionDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null)
					_descriptors = new ConcurrentDictionary<string, EndpointConnectionDescriptor>(StringComparer.OrdinalIgnoreCase);

				return _descriptors;
			}
		}
	}
}
