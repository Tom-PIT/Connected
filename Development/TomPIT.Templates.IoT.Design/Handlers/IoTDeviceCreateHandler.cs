using System;
using TomPIT.Design;

namespace TomPIT.IoT.Handlers
{
	internal class IoTDeviceCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(object instance)
		{
			var d = instance as IoTDevice;

			d.AuthenticationToken = Guid.NewGuid().AsString();
		}
	}
}
