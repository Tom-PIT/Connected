using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public interface IExecutionContext
	{
		IContextServices Services { get; }
		IMicroService MicroService { get; }

		JObject Invoke([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, [CodeAnalysisProvider(ExecutionContext.ApiParameterProvider)]JObject e, IApiTransaction transaction);
		JObject Invoke([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, [CodeAnalysisProvider(ExecutionContext.ApiParameterProvider)]JObject e);

		JObject Invoke([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, IApiTransaction transaction);
		JObject Invoke([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api);

		R Invoke<R>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, [CodeAnalysisProvider(ExecutionContext.ApiParameterProvider)]JObject e, IApiTransaction transaction);
		R Invoke<R>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, [CodeAnalysisProvider(ExecutionContext.ApiParameterProvider)]JObject e);

		R Invoke<R>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, IApiTransaction transaction);
		R Invoke<R>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api);

		R Invoke<R, A>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, A e, IApiTransaction transaction);
		R Invoke<R, A>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, A e);

		void Invoke<A>([CodeAnalysisProvider(ExecutionContext.ApiProvider)]string api, A e);

		RuntimeException Exception(string message);
		RuntimeException Exception(string format, string message);
		RuntimeException Exception(string message, int eventId);
		RuntimeException Exception(string format, string message, int eventId);
	}
}
