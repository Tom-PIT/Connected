using Newtonsoft.Json;
using System;

namespace TomPIT.Middleware
{
	public abstract class LifetimeMiddleware : MiddlewareOperation, ILifetimeMiddleware
	{
		[JsonIgnore]
		public TimeSpan Ping { get; protected set; }

		[JsonIgnore]
		public TimeSpan Lifespan { get; protected set; }

		[JsonIgnore]
		public RetryBehavior Retry { get; protected set; } = RetryBehavior.Retry;
	}
}
