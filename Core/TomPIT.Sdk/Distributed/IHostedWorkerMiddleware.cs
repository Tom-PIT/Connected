﻿using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public interface IHostedWorkerMiddleware : IMiddlewareComponent
	{
		void Invoke();
	}
}