using System;

namespace TomPIT.Middleware
{
	public interface IApi : IMiddlewareObject
	{
		Version Version { get; }
	}
}
