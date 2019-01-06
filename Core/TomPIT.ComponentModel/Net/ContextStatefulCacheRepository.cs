using TomPIT.Caching;

namespace TomPIT.Net
{
	public class ContextStatefulCacheRepository<T, K> : StatefulCacheRepository<T, K> where T : class
	{
		protected ContextStatefulCacheRepository(ISysContext server, string key) : base(server.Cache, key)
		{
			Server = server;
		}

		protected ISysContext Server { get; }
	}
}
