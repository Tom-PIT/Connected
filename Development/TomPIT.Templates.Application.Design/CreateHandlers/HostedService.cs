
using TomPIT.ComponentModel.Distributed;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class HostedService : ComponentCreateHandler<IHostedServiceConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.HostedWorker.txt";
	}
}