namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareService
	{
		IMiddlewareEntity CreateInstance(string type);
	}
}
