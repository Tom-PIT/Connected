namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareIoCService
	{
		IMiddlewareIoC UseMiddleware(string type);
	}
}
