using Newtonsoft.Json.Linq;

namespace TomPIT.Marketplace
{
	public interface IMarketplaceProxy
	{
		IPublisher Publisher { get; }

		JObject Countries { get; }
	}
}
