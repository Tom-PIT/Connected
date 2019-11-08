using System;
using TomPIT.Design;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.IoT.Design.Handlers
{
	internal class IoTDeviceCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IMiddlewareContext context, object instance)
		{
			var d = instance as IoTDevice;

			d.AuthenticationToken = Guid.NewGuid().ToString();
		}
	}
}
