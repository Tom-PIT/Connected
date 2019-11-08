using System;

namespace TomPIT.Worker.Services
{
	internal class EventDescriptor
	{
		public string Arguments { get; set; }
		public string Name { get; set; }
		public string Callback { get; set; }
		public Guid MicroService { get; set; }
	}
}
