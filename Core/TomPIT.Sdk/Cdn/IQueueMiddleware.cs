﻿using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public enum QueueValidationBehavior
	{
		Retry = 1,
		Complete = 2
	}
	public interface IQueueMiddleware : IMiddlewareComponent
	{
		void Invoke();

		QueueValidationBehavior ValidationFailed { get; }
	}
}