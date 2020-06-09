using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Middleware.Services;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareContext
	{
		IMiddlewareServices Services { get; }
		IMiddlewareEnvironment Environment { get; }
		ITenant Tenant { get; }
		IMiddlewareInterop Interop { get; }

		IDataReader<T> OpenReader<T>(IDataConnection connection, [CIP(CIP.CommandTextProvider)]string commandText);
		IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)]string commandText);

		IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText, ConnectionBehavior behavior);
		IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText, ConnectionBehavior behavior);
		IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)]string connection, ConnectionBehavior behavior, object arguments);
	}
}