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
		IDataConnection OpenConnection([CodeAnalysisProvider(OperationArguments.ConnectionProvider)]string connection);

		IDataReader<T> OpenReader<T>(IDataConnection connection, [CodeAnalysisProvider(OperationArguments.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter(IDataConnection connection, [CodeAnalysisProvider(OperationArguments.CommandTextProvider)]string commandText);

		IDataReader<T> OpenReader<T>([CodeAnalysisProvider(OperationArguments.ConnectionProvider)]string connection, [CodeAnalysisProvider(OperationArguments.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter([CodeAnalysisProvider(OperationArguments.ConnectionProvider)]string connection, [CodeAnalysisProvider(OperationArguments.CommandTextProvider)]string commandText);
	}
}
