using Amt.Sdk.Filters;
using Amt.Sys.Model.DataHub;
using System.Collections.Generic;
using System.Web.Http;

namespace Amt.DataHub.Controllers
{
	[ManagementAuthentication]
	public class DataHubController : ApiController
	{
		[HttpPost]
		public void DeleteOrphanedWorkers()
		{
			DataHubModel.DeleteOrphanedWorkers();
		}

		[HttpPost]
		public void DeletePostponedTransaction(int id)
		{
			var transaction = DataHubModel.SelectPostponedTransaction(id);

			if (transaction == null)
				return;

			DataHubModel.DeletePostponedTransaction(transaction.PopReceipt);
			Storage.DeletePostponed(transaction.FileId);
		}
		[HttpGet]
		public List<PostponedTransactionTask> QueryPostponedTransactionsTask()
		{
			return DataHubModel.QueryPostponedTransactionsTask();
		}
		[HttpGet]
		public List<Transaction> QueryTransactions()
		{
			return DataHubModel.QueryTransactions();
		}
		[HttpPost]
		public void RemoveWorkerInstance(string connectionId)
		{
			DataHubModel.RemoveWorkerInstance(connectionId);
		}
	}
}