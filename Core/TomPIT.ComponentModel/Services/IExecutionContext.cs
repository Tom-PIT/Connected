using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public interface IExecutionContext
	{
		IContextServices Services { get; }
		IContextIdentity Identity { get; }

		JObject Invoke(string api, JObject e, IApiTransaction transaction);
		JObject Invoke(string api, JObject e);

		JObject Invoke(string api, IApiTransaction transaction);
		JObject Invoke(string api);

		T Invoke<T>(string api, JObject e, IApiTransaction transaction);
		T Invoke<T>(string api, JObject e);

		T Invoke<T>(string api, IApiTransaction transaction);
		T Invoke<T>(string api);
	}
}
