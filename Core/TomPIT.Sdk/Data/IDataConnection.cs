using System;
using Newtonsoft.Json.Linq;
using TomPIT.Data.DataProviders;

namespace TomPIT.Data
{
	public interface IDataConnection : IDisposable
	{
		void Commit();
		void Rollback();

		void Open();
		void Close();

		void Execute(IDataCommandDescriptor command);
		JObject Query(IDataCommandDescriptor command);

		ConnectionBehavior Behavior { get; }
	}
}
