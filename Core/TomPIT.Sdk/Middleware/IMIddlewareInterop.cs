using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareInterop
	{
		R Invoke<R>([CAP(CAP.ApiProvider)]string api);

		R Invoke<R, A>([CAP(CAP.ApiProvider)]string api, A e);

		void Invoke<A>([CAP(CAP.ApiProvider)]string api, A e);

	}
}
