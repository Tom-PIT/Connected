using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IDependencyInjectionObject : IMiddlewareObject
	{
		IMiddlewareOperation Operation { get; }
		void Validate();
		void Authorize();
		void Commit();
		void Rollback();
	}
	public interface IDependencyInjectionMiddleware : IDependencyInjectionObject
	{
		void Invoke(object e);
	}

	public interface IDependencyInjectionMiddleware<T> : IDependencyInjectionObject
	{
		T Invoke(T e);
		T Authorize(T e);
	}
}
