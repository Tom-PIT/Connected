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
	[Obsolete]
	public interface IDataModelContext : IExecutionContext
	{
		IDataConnection OpenConnection([CodeAnalysisProvider(CodeAnalysisProviderAttribute.ConnectionProvider)]string connection);

		IDataReader<T> OpenReader<T>(	IDataConnection connection, 
												[CodeAnalysisProvider(CodeAnalysisProviderAttribute.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter(	IDataConnection connection, 
										[CodeAnalysisProvider(CodeAnalysisProviderAttribute.CommandTextProvider)]string commandText);

		IDataReader<T> OpenReader<T>(	[CodeAnalysisProvider(CodeAnalysisProviderAttribute.ConnectionProvider)]string connection, 
												[CodeAnalysisProvider(CodeAnalysisProviderAttribute.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter(	[CodeAnalysisProvider(CodeAnalysisProviderAttribute.ConnectionProvider)]string connection, 
										[CodeAnalysisProvider(CodeAnalysisProviderAttribute.CommandTextProvider)]string commandText);
	}
}
