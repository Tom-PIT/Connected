using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Services
{
	public interface ITransactionService
	{
		void Prepare(IBigDataApi api, JArray items);
		Guid Insert(Guid api, int blockCount);
		void InsertBlock(Guid transaction, JArray items);
		void Delete(Guid token);
		void Activate(Guid token);
	}
}
