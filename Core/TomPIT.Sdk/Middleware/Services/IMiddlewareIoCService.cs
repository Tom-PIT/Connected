namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareIoCService
	{
		T UseMiddleware<T>() where T : class;
		T UseMiddleware<T, A>(A arguments) where T : class;
	}
}
