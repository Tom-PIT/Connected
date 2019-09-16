using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public interface IIoTTransactionMiddleware : IMiddlewareComponent
	{
		void Invoke();
	}
}
