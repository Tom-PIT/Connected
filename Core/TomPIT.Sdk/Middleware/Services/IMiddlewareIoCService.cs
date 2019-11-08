using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareIoCService
	{
		R Invoke<R>([CIP(CIP.IoCOperationsProvider)]string middlewareOperation);
		void Invoke([CIP(CIP.IoCOperationsProvider)]string middlewareOperation);

		R Invoke<R>([CIP(CIP.IoCOperationsProvider)]string middlewareOperation, [CIP(CIP.IoCOperationParametersProvider)]object e);
		void Invoke([CIP(CIP.IoCOperationsProvider)]string middlewareOperation, [CIP(CIP.IoCOperationParametersProvider)]object e);
	}
}
