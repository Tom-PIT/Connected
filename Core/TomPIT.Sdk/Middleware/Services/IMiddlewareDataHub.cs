using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareDataHub
	{
		string Server { get; }
		void Notify([CIP(CIP.DataHubEndpointProvider)]string dataHubEndpoint, object e);
	}
}
