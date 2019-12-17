using System;

namespace TomPIT.Middleware
{
	public interface IMiddlewareTransaction
	{
		Guid Id { get; }
	}
}
