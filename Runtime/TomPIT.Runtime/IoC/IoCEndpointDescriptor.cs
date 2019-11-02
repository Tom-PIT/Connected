using System;

namespace TomPIT.IoC
{
	internal class IoCEndpointDescriptor
	{
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public Type Type { get; set; }
	}
}
