using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Caching
{
	public class ContextStatefulCacheRepository<T, K> : StatefulCacheRepository<T, K> where T : class
	{
		protected ContextStatefulCacheRepository(ISysConnection connection, string key) : base(connection.Cache, key)
		{
			Connection = connection;
		}

		protected ISysConnection Connection { get; }
	}
}
