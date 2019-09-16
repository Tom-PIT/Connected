using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Middleware.Services;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareContext
	{
		IMiddlewareServices Services { get; }
		IMicroService MicroService { get; }
		ITenant Tenant { get; }

		R Invoke<R>([CAP(CAP.ApiProvider)]string api);

		R Invoke<R, A>([CAP(CAP.ApiProvider)]string api, A e);

		void Invoke<A>([CAP(CAP.ApiProvider)]string api, A e);

		IDataConnection OpenConnection([CAP(CAP.ConnectionProvider)]string connection);

		IDataReader<T> OpenReader<T>(IDataConnection connection, [CAP(CAP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter(IDataConnection connection, [CAP(CAP.CommandTextProvider)]string commandText);

		IDataReader<T> OpenReader<T>([CAP(CAP.ConnectionProvider)]string connection, [CAP(CAP.CommandTextProvider)]string commandText);
		IDataWriter OpenWriter([CAP(CAP.ConnectionProvider)]string connection, [CAP(CAP.CommandTextProvider)]string commandText);
	}
}