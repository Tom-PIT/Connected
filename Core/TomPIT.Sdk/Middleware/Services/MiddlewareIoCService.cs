using TomPIT.IoC;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareIoCService : MiddlewareComponent, IMiddlewareIoCService
	{
		public MiddlewareIoCService(IMiddlewareContext context) : base(context)
		{
		}

		public T UseMiddleware<T>() where T : class
		{
			return Context.Tenant.GetService<IIoCService>().CreateMiddleware<T>();
		}

		public T UseMiddleware<T, A>(A arguments) where T : class
		{
			return Context.Tenant.GetService<IIoCService>().CreateMiddleware<T, A>(arguments);
		}
	}
}
