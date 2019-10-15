using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareInterop
	{
		R Invoke<R>([CIP(CIP.ApiOperationProvider)]string api);
		dynamic Invoke([CIP(CIP.ApiOperationProvider)]string api);

		R Invoke<R, A>([CIP(CIP.ApiOperationProvider)]string api, [CIP(CIP.ApiOperationParameterProvider)]A e);

		dynamic Invoke<A>([CIP(CIP.ApiOperationProvider)]string api, [CIP(CIP.ApiOperationParameterProvider)]A e);
	}
}
