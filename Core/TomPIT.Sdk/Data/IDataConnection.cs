using System;
using System.Collections.Generic;
using System.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public interface IDataConnection : IDisposable
	{
		void Commit();
		void Rollback();

		void Open();
		void Close();

		int Execute(IDataCommandDescriptor command);
		List<T> Query<T>(IDataCommandDescriptor command);
		T Select<T>(IDataCommandDescriptor command);

		IDbCommand CreateCommand();
		ConnectionBehavior Behavior { get; }

		//IDbConnection Connection { get; }
		IDbTransaction Transaction { get; set; }
		ICommandTextParser Parser { get; }

		ConnectionState State { get; }
		IMiddlewareContext Context { get; }
	}
}
