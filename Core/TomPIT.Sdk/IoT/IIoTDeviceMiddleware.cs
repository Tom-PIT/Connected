using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public interface IIoTDeviceMiddleware : IMiddlewareComponent
	{
		[JsonIgnore]
		JObject Arguments { get; }
		void Invoke();
	}
}
