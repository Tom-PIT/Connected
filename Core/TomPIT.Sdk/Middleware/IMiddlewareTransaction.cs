using System;

namespace TomPIT.Middleware
{
	public interface IMiddlewareTransaction
	{
		Guid Id { get; }
		string Name { get; }

		void Commit();
		void Rollback();

		void Notify(IMiddlewareOperation operation);
	}
}
