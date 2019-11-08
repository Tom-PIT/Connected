using System;

namespace TomPIT.Middleware
{
	public class MiddlewareCallback : IMiddlewareCallback
	{
		public MiddlewareCallback(Guid microService, Guid component, Guid element)
		{
			MicroService = microService;
			Component = component;
			Element = element;
		}

		public Guid MicroService { get; }
		public Guid Component { get; }
		public Guid Element { get; }

		internal bool Attached { get; set; }
	}
}
