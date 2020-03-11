using System;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Middleware.Services;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareContext
	{
		Guid Id { get; }
		IMiddlewareServices Services { get; }
		IMiddlewareEnvironment Environment { get; }
		ITenant Tenant { get; }
		IMiddlewareInterop Interop { get; }

		IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)]string connection, [CIP(CIP.CommandTextProvider)]string commandText);
	}
}