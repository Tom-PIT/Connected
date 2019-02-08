using System;
using TomPIT.Design;
using TomPIT.Services;

namespace TomPIT.IoT.Handlers
{
	internal class IoTDeviceCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IExecutionContext context, object instance)
		{
			var d = instance as IoTDevice;

			d.AuthenticationToken = Guid.NewGuid().AsString();
		}
	}
}
