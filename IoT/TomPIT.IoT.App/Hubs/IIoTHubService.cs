using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT.Hubs
{
	public interface IIoTHubService
	{
		IIoTDevice SelectDevice(string authenticationToken);
		JObject SetData(IIoTDevice device, JObject data);
		IIoTSchemaConfiguration SelectSchema(IIoTHubConfiguration hub);
		void FlushChanges();
	}
}
