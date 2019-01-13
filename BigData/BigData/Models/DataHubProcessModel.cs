using Amt.ComponentModel.Dev;
using Amt.Core.Dev;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Models;
using System;

namespace Amt.DataHub.Models
{
	internal class DataHubProcessModel : ApiModel
	{
		public DataHubProcessModel()
		{

		}

		public void DataBind(Guid worker)
		{
			var components = AmtShell.GetService<IComponentService>().Query(InProcessWorker.TypeId);

			if (components == null)
			{
				Log.Warning(this, "InProcess worker not found.", LogEvents.DhInProcessWorkerNull);
				return;
			}

			IComponentReference reference = null;

			foreach (var i in components)
			{
				var ep = AmtShell.GetService<IConfigurationService>().Select<WorkerBase>(i.Identifier);

				if (ep.RefId == worker)
				{
					reference = i;

					break;
				}
			}

			if (reference == null)
			{
				Log.Warning(this, "InProcess worker not found.", LogEvents.DhInProcessWorkerNull);
				return;
			}

			var component = AmtShell.GetService<IComponentService>().Select(reference.Identifier);

			if (component == null)
			{
				Log.Warning(this, "InProcess worker not found.", LogEvents.DhInProcessWorkerNull, reference.Name);
				return;
			}

			var _solution = AmtShell.GetService<ISolutionService>().Select(component.SolutionId);

			DataBind(_solution.Id);
		}
	}
}