using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.BigData
{
	public enum TimestampBehavior
	{
		Static = 1,
		Dynamic = 2
	}

	public interface IPartitionMiddleware<T> : IMiddlewareComponent
	{
		TimestampBehavior Timestamp { get; }

		List<T> Invoke(List<T> items);
	}
}
