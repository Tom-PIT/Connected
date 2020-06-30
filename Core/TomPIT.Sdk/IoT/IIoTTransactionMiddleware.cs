using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public interface IIoTTransactionMiddleware : IMiddlewareComponent
	{
		void Invoke(IIoTDeviceMiddleware device);

		string Name { get; }
	}
}
