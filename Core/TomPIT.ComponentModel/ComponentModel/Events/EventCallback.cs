using System;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel
{
	internal class EventCallback : IEventCallback
	{
		public EventCallback(Guid microService, Guid api, Guid operation)
		{
			MicroService = microService;
			Api = api;
			Operation = operation;
		}

		public Guid MicroService { get; }
		public Guid Api { get; }
		public Guid Operation { get; }
	}
}
