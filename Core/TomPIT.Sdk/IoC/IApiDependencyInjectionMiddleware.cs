using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IApiDependencyInjectionObject : IMiddlewareObject
	{
		IMiddlewareOperation Operation { get; }
		void Validate();
		void Authorize();
		void Commit();
		void Rollback();
	}
	public interface IApiDependencyInjectionMiddleware : IApiDependencyInjectionObject
	{
		void Invoke(object e);
	}

	public interface IApiDependencyInjectionMiddleware<T> : IApiDependencyInjectionObject
	{
		T Invoke(T e);
		T Authorize(T e);
	}
}
