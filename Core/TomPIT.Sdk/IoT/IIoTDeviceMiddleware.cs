using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public interface IIoTDeviceMiddleware : IMiddlewareComponent
	{
		void Invoke();

		List<IIoTTransactionMiddleware> Transactions { get; }
	}
}
