using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public abstract class IoTDeviceHandler : ProcessHandler, IIoTDeviceHandler
	{
		protected IoTDeviceHandler(IDataModelContext context, JObject arguments) : base(context)
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
