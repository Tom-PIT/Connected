using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public interface IIoTDeviceHandler : IProcessHandler
	{
		[JsonIgnore]
		JObject Arguments { get; }
		void Invoke();
	}
}
