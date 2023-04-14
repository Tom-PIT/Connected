using System;

namespace TomPIT.Middleware
{
	public enum RetryBehavior
	{
		NoRetry = 1,
		Retry = 2
	}

	public interface ILifetimeMiddleware
	{
		TimeSpan Ping { get; }
		TimeSpan Lifespan { get; }
		RetryBehavior Retry { get; }
	}
}
