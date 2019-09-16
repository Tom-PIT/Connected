using System;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.Data.DataProviders;

namespace TomPIT.Data
{
	public interface IDataConnection : IDisposable
	{
		void Begin();
		void Begin(IsolationLevel isolationLevel);
		void Commit();
		void Rollback();

		void Open();
		void Close();

		void Execute(IDataCommandDescriptor command);
		JObject Query(IDataCommandDescriptor command);
	}
}
