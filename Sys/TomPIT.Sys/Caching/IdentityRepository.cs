using System.Threading;
using TomPIT.Caching;

namespace TomPIT.Sys.Caching
{
	internal class IdentityRepository<T, K> : PersistentRepository<T, K> where T : class
	{
		private long _identity = 0L;
		public IdentityRepository(IMemoryCache container, string key) : base(container, key)
		{
		}

		protected long Identity => _identity;

		protected void Seed(long identity)
		{
			Interlocked.Exchange(ref _identity, identity);
		}

		protected long Increment()
		{
			return Interlocked.Increment(ref _identity);
		}
	}
}
