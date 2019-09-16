using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public abstract class IoTDeviceMiddleware : MiddlewareComponent, IIoTDeviceMiddleware
	{
		protected IoTDeviceMiddleware(IMiddlewareContext context, JObject arguments) : base(context)
		{
			Arguments = arguments;
		}

		[JsonIgnore]
		public JObject Arguments { get; }

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
