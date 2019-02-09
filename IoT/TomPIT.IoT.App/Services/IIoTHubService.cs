using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT.Services
{
	public interface IIoTHubService
	{
		IIoTDevice SelectDevice(string authenticationToken);
		JObject SetData(IIoTDevice device, JObject data);
		IIoTSchema SelectSchema(IIoTHub hub);
		void FlushChanges();
	}
}
