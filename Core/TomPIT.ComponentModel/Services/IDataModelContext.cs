using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Apis;
using TomPIT.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.Services
{
	public interface IDataModelContext : IExecutionContext
	{
		JObject DataSource([CodeAnalysisProvider(OperationArguments.DataSourceProvider)]string dataSource,
			[CodeAnalysisProvider(OperationArguments.DataSourceParameterProvider)]JObject arguments);

		JObject DataSource([CodeAnalysisProvider(OperationArguments.DataSourceProvider)]string dataSource);

		T DataSource<T>([CodeAnalysisProvider(OperationArguments.DataSourceProvider)]string dataSource,
			[CodeAnalysisProvider(OperationArguments.DataSourceParameterProvider)]JObject arguments);

		T DataSource<T>([CodeAnalysisProvider(OperationArguments.DataSourceProvider)]string dataSource);

		JObject Transaction([CodeAnalysisProvider(OperationArguments.TransactionProvider)]string transaction,
			[CodeAnalysisProvider(OperationArguments.TransactionParameterProvider)]JObject arguments);

		JObject Transaction([CodeAnalysisProvider(OperationArguments.TransactionProvider)]string transaction);

		JObject Transaction(IDataConnection connection, [CodeAnalysisProvider(OperationArguments.TransactionProvider)]string transaction,
			[CodeAnalysisProvider(OperationArguments.TransactionParameterProvider)]JObject arguments);

		JObject Transaction(IDataConnection connection, [CodeAnalysisProvider(OperationArguments.TransactionProvider)]string transaction);

		IDataConnection OpenConnection([CodeAnalysisProvider(OperationArguments.ConnectionProvider)]string connection);

		IDataReader<T> OpenReader<T>(IDataConnection connection, string commandText);
		IDataWriter OpenWriter(IDataConnection connection, string commandText);

		IDataReader<T> OpenReader<T>([CodeAnalysisProvider(OperationArguments.ConnectionProvider)]string connection, string commandText);
		IDataWriter OpenWriter([CodeAnalysisProvider(OperationArguments.ConnectionProvider)]string connection, string commandText);
	}
}
