using System.Collections.Generic;

namespace TomPIT.IoT
{
	public interface IIoTHubMiddleware<TSchema> where TSchema : class
	{
		TSchema Schema { get; }
		List<IIoTTransactionMiddleware> Transactions { get; }
		List<IIoTDeviceMiddleware> Devices { get; }

		void Authorize(IoTConnectionArgs e);
	}
}
