using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Caching
{
	public abstract class ClientRepository<T, K> : CacheRepository<T, K> where T : class
	{
		protected ClientRepository(ISysConnection connection, string key) : base(connection.Cache, key)
		{
			Connection = connection;
		}

		protected ISysConnection Connection { get; }
	}
}
