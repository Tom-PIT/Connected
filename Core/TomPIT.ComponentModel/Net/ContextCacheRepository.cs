using TomPIT.Caching;

namespace TomPIT.Net
{
	public abstract class ContextCacheRepository<T, K> : CacheRepository<T, K> where T : class
	{
		protected ContextCacheRepository(ISysContext server, string key) : base(server.Cache, key)
		{
			Server = server;
		}

		protected ISysContext Server { get; }
	}
}
