using System;

namespace TomPIT.Middleware
{
	public interface IMiddlewareCallback
	{
		Guid MicroService { get; }
		Guid Component { get; }
		Guid Element { get; }
	}
}
