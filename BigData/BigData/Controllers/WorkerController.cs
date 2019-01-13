using Amt.DataHub.Data;
using Amt.DataHub.Models;
using Amt.DataHub.Partitions;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Filters;
using Amt.Sdk.Runtime;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Amt.DataHub.Controllers
{
	[ManagementAuthentication]
	public class WorkerController : ApiController
	{
		[HttpPost]
		public void RunTask(Guid popReceipt)
		{
			try
			{
				var task = DataHubModel.TakeTransactionTask(popReceipt);

				if (task == null)
					return;

				var worker = AmtShell.GetService<IConfigurationService>().Select<WorkerBase>(task.Worker);

				if (worker == null)
				{
					Log.Warning(this, "Task worker not found.", LogEvents.DhTaskWorkerNull, task.Id.AsString(), task.BlockId.AsString(), task.Worker.AsString());

					DataHubModel.CompleteTask(task.PopReceipt);
				}
				else
				{
					if (worker is DistributedWorker)
					{
						var w = DataHubModel.SelectIdleWorker(worker.RefId);

						if (w != null)
						{
							var args = new TaskArgs();

							args.Id = task.Identifier;
							//TODO:implement connection url so we'll be able to call endpoint where client has physical connection.
							//HubContext.DataHubWorkers.Clients.Client(w.ConnectionId).startTask(args);
						}
						else
							Log.Warning(this, "No distributed idle worker(s) available.", LogEvents.DhNoIdleWorker, worker.RefId.AsString());
					}
					else if (worker is PartitionWorker)
					{
						var w = new PartitionWorkerRuntime();

						try
						{
							if (w.ProcessData(worker as PartitionWorker, task))
								DataHubModel.CompleteTask(task.PopReceipt);
							else
								DataHubModel.CancelTransactionTask(task.PopReceipt, null);
						}
						catch (Exception ex)
						{
							DataHubModel.CancelTransactionTask(task.PopReceipt, null);
							Log.Error(this, ex, LogEvents.DhPartitionWorkerRun, worker.RefId.AsString());
						}
					}
					else if (worker is InProcessWorker)
					{
						var iw = worker as InProcessWorker;

						var model = new DataHubProcessModel();

						try
						{
							model.DataBind(worker.RefId);

							var data = DataHubProxy.ProcessTask(task.Identifier, 30);

							if (data == null)
								DataHubModel.CompleteTask(task.PopReceipt);
							else
							{
								var args = Compiler.Execute(iw.Execute, this, new ProcessArgs(model, task.Id, data, task.PopReceipt));

								if (!args.Cancel)
									DataHubModel.CompleteTask(task.PopReceipt);
							}
						}
						catch (Exception ex)
						{
							DataHubModel.CancelTransactionTask(task.PopReceipt, null);
							Log.Error(this, ex, LogEvents.DhInProcessWorkerRun, worker.RefId.AsString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(this, ex, 0);
			}
		}

		[HttpPost]
		public void CleanCompletedTransactions()
		{
			var completed = DataHubModel.QueryCompletedTransactions();

			var cs = new ContextState(null);

			Parallel.ForEach(completed, (i) =>
			{
				cs.Attach();

				DataHubModel.DeleteTransaction(i.Id);
			});
		}

		[HttpPost]
		public void RunPostponed(Guid popReceipt)
		{
			var transaction = DataHubModel.TakePostponedTransaction(popReceipt);

			if (transaction == null)
				return;

			var partition = PartitionModel.SelectPartition(transaction.PartitionId);

			if (partition == null)
			{
				RemovePostponedTransaction(transaction);
				return;
			}

			var task = DataHubModel.SelectTransactionTask(transaction.TaskId);

			if (task == null)
			{
				RemovePostponedTransaction(transaction);
				return;
			}

			var data = Storage.LoadPostponed(transaction.FileId);

			if (data == null)
			{
				RemovePostponedTransaction(transaction);
				return;
			}

			try
			{
				if (PartitionModel.Update(partition.Identifier, task.Id, data, TransactionMode.Postponed, transaction.Id, task.PopReceipt))
					RemovePostponedTransaction(transaction);
				else
					DataHubModel.ResetPostponedTransaction(popReceipt);
			}
			catch
			{
				DataHubModel.ResetPostponedTransaction(popReceipt);
			}
		}

		[HttpPost]
		public void CalculateNodeSize()
		{
			AmtShell.GetService<INodeService>().Recalculate();
		}

		[HttpPost]
		public void RunDependency(Guid popReceipt)
		{
			try
			{
				var workerDependency = DataHubModel.TakeWorkerDependecy(popReceipt);

				if (workerDependency == null)
					return;

				var worker = DataHubModel.SelectWorker(workerDependency.Worker);

				if (worker == null)
				{
					RemoveWorkerDependency(workerDependency.Id, null);
					return;
				}

				List<WorkerDependencyFile> files = null;

				var dataTable = PartitionModel.LoadWorkerData(workerDependency.Id, out files);

				if (dataTable == null)
				{
					RemoveWorkerDependency(workerDependency.Id, files);
					return;
				}

#if DUMP
			Dump.RunDependency(workerDependency.Id, workerDependency.Partition, dataTable);
#endif

				var t = new DependencyPipelineTransaction(worker, dataTable);

				t.Start();

				RemoveWorkerDependency(workerDependency.Id, files);
			}
			catch (Exception ex)
			{
				Log.Info("Worker", popReceipt.AsString() + "\r\n" + ex.Message, "9");
			}
		}

		[HttpPost]
		public void DeletePartitions()
		{
			PartitionModel.DeletePartitions();
		}

		[HttpPost]
		public void SynchronizePartitions()
		{
			PartitionModel.SynchronizePartitions();
		}

		private void RemoveWorkerDependency(long id, List<WorkerDependencyFile> files)
		{
			DataHubModel.DeleteWorkerDependecy(id);

			if (files == null || files.Count == 0)
				return;

			foreach (var file in files)
				Storage.DeleteDependecy(file.FileId);
		}

		private void RemovePostponedTransaction(PostponedTransaction transaction)
		{
			DataHubModel.DeletePostponedTransaction(transaction.PopReceipt);
			Storage.DeletePostponed(transaction.FileId);
		}

		[HttpGet]
		public List<WorkerInstance> QueryWorkerInstances()
		{
			return DataHubModel.QueryWorkerInstance();
		}
	}
}