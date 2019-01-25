using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Deployment;

namespace TomPIT.Marketplace
{
	public interface IMarketplaceProxy
	{
		IPublisher Publisher { get; }
		JObject Countries { get; }
		IPackage Package { get; }
		IMicroService MicroService { get; }
		JArray Tags { get; }
	}
}
