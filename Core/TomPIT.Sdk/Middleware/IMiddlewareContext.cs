using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Middleware.Services;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareContext
	{
		IMiddlewareServices Services { get; }

		ITenant Tenant { get; }

		IMiddlewareInterop Interop { get; }
		IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)]string connection);

		IDataReader<T> OpenReader<T>(IDataConnection connection, [CAP(CAP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter(IDataConnection connection, [CAP(CAP.CommandTextProvider)]string commandText);

		IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, [CAP(CAP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, [CAP(CAP.CommandTextProvider)]string commandText);
	}
}