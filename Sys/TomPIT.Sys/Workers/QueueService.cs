using System;
using System.Threading.Tasks;
using TomPIT.Services;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Workers
{
	internal class QueueService : HostedService
	{
		public QueueService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}
		protected override Task Process()
		{
			try
			{
				var groups = DataModel.ResourceGroups.Query();

				Parallel.ForEach(groups,
					(i) =>
					{
			//			ProcessResourceGroup(i);
					});
			}
			catch
			{
				//TODO: log exception
			}

			return Task.CompletedTask;
		}

		//private void ProcessResourceGroup(IServerResourceGroup resourceGroup)
		//{
		//	while (DataModel.Workers.Dequeue(resourceGroup, out IQueueMessage m))
		//	{
		//		var url = DataModel.InstanceEndpoints.Url(InstanceType.Worker, InstanceVerbs.Post);

		//		if (url == null)
		//			DataModel.Logging.Insert(Logging.CategoryWorker, nameof(ProcessResourceGroup), string.Format("{0} ({1}, {2})", SR.ErrInstanceNull, InstanceType.Worker, InstanceVerbs.Post), System.Diagnostics.TraceLevel.Warning, SysEvents.WorkerDequeue);


		//	}
		//}
	}
}
