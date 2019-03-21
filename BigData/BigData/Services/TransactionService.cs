using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.BigData.Services
{
	internal class TransactionService : ServiceBase, ITransactionService
	{
		public TransactionService(ISysConnection connection) : base(connection)
		{
		}

		public void Activate(Guid token)
		{
			var u = Connection.CreateUrl("BigDataManagement", "ActivateTransaction");
			//var e=new JObject
			//{
			//	{"" }
			//}
		}

		public void Delete(Guid token)
		{
			throw new NotImplementedException();
		}

		public Guid Insert(Guid api, int blockCount)
		{
			throw new NotImplementedException();
		}

		public void InsertBlock(Guid transaction, JArray items)
		{
			throw new NotImplementedException();
		}

		public void Prepare(IBigDataApi api, JArray items)
		{
			new TransactionPreparer(api, items).Prepare();
		}
	}
}
