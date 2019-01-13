using Amt.ComponentModel.Collections;
using Amt.Sdk.DataHub;
using System.Data;

namespace Amt.DataHub.Transactions
{
	public class DependencyPipelineTransaction : PipelineTransaction
	{
		private WorkerBase _worker = null;

		public DependencyPipelineTransaction(WorkerBase worker, DataTable content) : base(content)
		{
			_worker = worker;
		}

		protected override ListItems<EndpointWorker> Workers
		{
			get
			{
				if (_worker == null)
					return null;

				return _worker.Workers;
			}
		}
	}
}