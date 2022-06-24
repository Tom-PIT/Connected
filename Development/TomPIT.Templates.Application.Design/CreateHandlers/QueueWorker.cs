
using TomPIT.ComponentModel.Distributed;
using TomPIT.Development.Handlers;
using TomPIT.Distributed;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class QueueWorker : ComponentCreateHandler<IQueueWorker>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.QueueWorker.txt";
	}
}