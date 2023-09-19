
using TomPIT.ComponentModel.Distributed;
using TomPIT.Development.Handlers;
using TomPIT.Distributed;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class HostedWorker : ComponentCreateHandler<IHostedWorkerConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.HostedWorker.txt";
	}
}