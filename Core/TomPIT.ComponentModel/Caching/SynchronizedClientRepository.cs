using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Caching
{
	public class SynchronizedClientRepository<T, K> : SynchronizedRepository<T, K> where T : class
	{
		protected SynchronizedClientRepository(ISysConnection connection, string key) : base(connection.Cache, key)
		{
			Connection = connection;
		}

		protected ISysConnection Connection { get; }
	}
}
