using Amt.Api.Data;
using Amt.ComponentModel.Dev;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Amt.DataHub
{
	public static class DataHubModel
	{
		private static Lazy<ConcurrentDictionary<Guid, WorkerBase>> _workers = new Lazy<ConcurrentDictionary<Guid, WorkerBase>>();

		static DataHubModel()
		{
			AmtShell.GetService<IConfigurationService>().ConfigurationChanged += OnConfigurationChanged;
		}

		private static void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
		{
			if ((e.Component.Type == InProcessWorker.TypeId || e.Component.Type == PartitionWorker.TypeId || e.Component.Type == DistributedWorker.TypeId)
					&& e.Configuration != null)
			{
				var instance = Workers.FirstOrDefault(f => f.Value.DesignId == e.Configuration.DesignId);
				WorkerBase ep = null;

				if (instance.Value != null)
					Workers.TryRemove(instance.Key, out ep);
			}
		}

		public static void CompleteTask(Guid popReceipt)
		{
			ActivateDependencies(popReceipt);

			var w = new Writer("amt_dh_transaction_task_complete");

			w.CreateParameter("@pop_receipt", popReceipt);

			w.Execute();
		}

		private static void ActivateDependencies(Guid popReceipt)
		{
			var task = SelectTransactionTaskByPopReceipt(popReceipt);

			if (task == null || !task.HasDependencies)
				return;

			var block = SelectTransactionBlock(task.BlockId);

			if (block == null || block.WorkerLeft > 1)
				return;

			var blocks = QueryBlocks(block.TransactionId);

			if (blocks != null)
			{
				foreach (var item in blocks)
				{
					//if we are not last task to complete dont do anything
					if (item.WorkerLeft > 0 && item.Id != block.Id)
						return;
				}
			}

			var worker = AmtShell.GetService<IConfigurationService>().Select<WorkerBase>(task.Worker);

			if (worker == null)
				return;

			QueueWorkerDependecy(worker.RefId, popReceipt);
		}

		public static void RegisterWorkerInstance(Guid worker, string connectionId)
		{
			var w = new Writer("amt_dh_worker_instance_ins");

			w.CreateParameter("@worker", worker);
			w.CreateParameter("@connection_id", connectionId);

			w.Execute();
		}

		public static void RemoveWorkerInstance(string connectionId)
		{
			var w = new Writer("amt_dh_worker_instance_del");

			w.CreateParameter("@connection_id", connectionId);

			w.Execute();
		}

		public static void RenewWorkerInstance(string connectionId)
		{
			var w = new Writer("amt_dh_worker_instance_renew");

			w.CreateParameter("@connection_id", connectionId);

			w.Execute();
		}

		public static WorkerBase SelectWorker(Guid id)
		{
			WorkerBase result = null;

			if (Workers.ContainsKey(id))
			{
				if (Workers.TryGetValue(id, out result))
					return result;
			}

			var comps = AmtShell.GetService<IComponentService>().Query(PartitionWorker.TypeId, InProcessWorker.TypeId, DistributedWorker.TypeId);

			foreach (var i in comps)
			{
				var ep = AmtShell.GetService<IConfigurationService>().Select<WorkerBase>(i.Identifier);

				if (ep.RefId == id)
				{
					result = ep;
					Workers.TryAdd(id, result);
					break;
				}
			}

			return result;
		}

		public static PostponedTransaction TakePostponedTransaction(Guid popReceipt)
		{
			var r = new Reader<PostponedTransaction>("amt_dh_transaction_postponed_take");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public static WorkerInstance SelectIdleWorker(Guid worker)
		{
			var r = new Reader<WorkerInstance>("amt_dh_worker_instance_sel_idle");

			r.CreateParameter("@worker", worker);

			return r.ExecuteSingleRow();
		}

		public static TransactionBlock SelectTransactionBlock(long id)
		{
			var r = new Reader<TransactionBlock>("amt_dh_transaction_block_sel");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}

		public static TransactionBlock SelectTransactionBlock(Guid identifier)
		{
			var r = new Reader<TransactionBlock>("amt_dh_transaction_block_sel");

			r.CreateParameter("@identifier", identifier);

			return r.ExecuteSingleRow();
		}

		public static TransactionTask SelectTransactionTask(Guid identifier)
		{
			var r = new Reader<TransactionTask>("amt_dh_transaction_task_sel");

			r.CreateParameter("@identifier", identifier);

			return r.ExecuteSingleRow();
		}

		public static WorkerDependency TakeWorkerDependecy(Guid popReceipt)
		{
			var r = new Reader<WorkerDependency>("amt_dh_worker_dependency_take");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public static TransactionTask TakeTransactionTask(Guid popReceipt)
		{
			var r = new Reader<TransactionTask>("amt_dh_transaction_task_take");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public static TransactionTask SelectTransactionTaskByPopReceipt(Guid popReceipt)
		{
			var r = new Reader<TransactionTask>("amt_dh_transaction_task_sel");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public static TransactionTask SelectTransactionTask(long id)
		{
			var r = new Reader<TransactionTask>("amt_dh_transaction_task_sel");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}

		public static bool StartTransactionTask(long id, int timeout)
		{
			/*
			 * should not be called
			 */
			var r = new ScalarReader<int>("amt_dh_transaction_task_start");

			r.CreateParameter("@id", id);
			r.CreateParameter("@timeout", timeout < 1 ? 30 : timeout);

			return r.ExecuteScalar(0) > 0;
		}

		public static void CancelTransactionTask(Guid popReceipt, string connectionId)
		{
			var w = new Writer("amt_dh_transaction_task_cancel");

			w.CreateParameter("@pop_receipt", popReceipt);
			w.CreateParameter("@connection_id", connectionId);

			w.Execute();
		}

		public static List<Transaction> QueryCompletedTransactions()
		{
			return new Reader<Transaction>("amt_dh_transaction_que_completed").Execute();
		}

		public static void DeleteTransaction(long id)
		{
			var blocks = QueryBlocks(id);

			foreach (var i in blocks)
				Storage.DeleteBlock(i.Identifier);

			var w = new Writer("amt_dh_transaction_del");

			w.CreateParameter("@id", id);

			w.Execute();
		}

		private static List<TransactionBlock> QueryBlocks(long transactionId)
		{
			var r = new Reader<TransactionBlock>("amt_dh_transaction_block_que");

			r.CreateParameter("@transaction_id", transactionId);

			return r.Execute();
		}

		public static long PostponeTransaction(int partitionId, long taskId)
		{
			var w = new LongWriter("amt_dh_transaction_postpone");

			w.CreateParameter("@partition_id", partitionId);
			w.CreateParameter("@task_id", taskId);

			w.Execute();

			return w.Result;
		}

		public static PostponedTransaction SelectPostponedTransaction(long id)
		{
			var r = new Reader<PostponedTransaction>("amt_dh_transaction_postponed_sel");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}

		public static void DeletePostponedTransaction(Guid popReceipt)
		{
			var w = new Writer("amt_dh_transaction_postponed_del");

			w.CreateParameter("@pop_receipt", popReceipt);

			w.Execute();
		}

		public static void ResetPostponedTransaction(Guid popReceipt)
		{
			var w = new Writer("amt_dh_transaction_postponed_reset");

			w.CreateParameter("@pop_receipt", popReceipt);

			w.Execute();
		}

		public static void QueueWorkerDependecy(Guid id, Guid taskPopReceipt)
		{
			var w = new Writer("amt_dh_worker_dependency_queue");

			w.CreateParameter("@worker", id);
			w.CreateParameter("@task_pop_receipt", taskPopReceipt);

			w.Execute();
		}

		public static WorkerDependency SelectWorkerDependecy(long id)
		{
			var r = new Reader<WorkerDependency>("amt_dh_worker_dependency_sel");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}

		public static void DeleteWorkerDependecy(long id)
		{
			var w = new Writer("amt_dh_worker_dependency_del");

			w.CreateParameter("@id", id);

			w.Execute();
		}

		public static List<WorkerDependencyFile> QueryWorkerDependencyFile(long id)
		{
			var r = new Reader<WorkerDependencyFile>("amt_dh_worker_dependency_file_que");

			r.CreateParameter("@id", id);

			return r.Execute();
		}

		public static List<WorkerInstance> QueryWorkerInstance()
		{
			return new Reader<WorkerInstance>("amt_dh_worker_instance_que").Execute();
		}

		public static List<Transaction> QueryTransactions()
		{
			return new Reader<Transaction>("amt_dh_transaction_que").Execute();
		}

		public static List<PostponedTransactionTask> QueryPostponedTransactionsTask()
		{
			return new Reader<PostponedTransactionTask>("amt_dh_transaction_postponed_que_task").Execute();
		}

		public static void DeleteOrphanedWorkers()
		{
			var workers = QueryWorkerInstance();

			foreach (var worker in workers)
			{
				if (worker.LastAccessTime.AddHours(1) <= DateTime.UtcNow)
					RemoveWorkerInstance(worker.ConnectionId);
			}
		}

		private static ConcurrentDictionary<Guid, WorkerBase> Workers { get { return _workers.Value; } }
	}
}