using TomPIT.ComponentModel.IoT;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.IoT.Design.CreateHandlers
{
	internal class IoTHub : ComponentCreateHandler<IIoTHubConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.IoT.Design.CreateHandlers.Templates.IoTHub.txt";
	}
}
