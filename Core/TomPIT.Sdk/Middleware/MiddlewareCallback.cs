using System;
using TomPIT.Annotations;

namespace TomPIT.Middleware
{
	public class MiddlewareCallback : IMiddlewareCallback
	{
		public MiddlewareCallback(IMiddlewareContext context, Guid microService, Guid component, Guid element)
		{
			Context = context;
			MicroService = microService;
			Component = component;
			Element = element;
		}

		[SkipValidation]
		public IMiddlewareContext Context { get; }
		public Guid MicroService { get; }
		public Guid Component { get; }
		public Guid Element { get; }

		internal bool Attached { get; set; }
	}
}
