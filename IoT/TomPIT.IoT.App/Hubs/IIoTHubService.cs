using Newtonsoft.Json.Linq;

namespace TomPIT.IoT.Hubs
{
	public interface IIoTHubService
	{
		JObject SetData(string device, object data);
		void FlushChanges();
	}
}
