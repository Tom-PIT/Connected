using TomPIT.Middleware;

namespace TomPIT.Data
{
	public enum ConnectionStringContext
	{
		User = 1,
		Elevated = 2
	}

	public interface IConnectionMiddleware : IMiddlewareComponent
	{
		ConnectionStringContext ConnectionContext { get; }
		IConnectionString Invoke(ConnectionMiddlewareArgs e);
	}
}
