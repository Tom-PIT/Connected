using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public abstract class IoTDeviceMiddleware : MiddlewareComponent, IIoTDeviceMiddleware
	{
		[JsonIgnore]
		public JObject Arguments { get; set; }

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
