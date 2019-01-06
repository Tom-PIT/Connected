using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Runtime.ApplicationContextServices;

namespace TomPIT.Runtime
{
	public interface IApplicationContext
	{
		IServices Services { get; }
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
