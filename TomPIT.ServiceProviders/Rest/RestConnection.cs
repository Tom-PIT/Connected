using TomPIT.Connectivity;

namespace TomPIT.ServiceProviders.Rest
{
	public abstract class RestConnection
	{
		protected static IHttpConnection GetConnection(string subscriptionKey)
		{
			return new HttpConnection(subscriptionKey);
		}
	}
}
