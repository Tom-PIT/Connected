using System;
using System.Collections.Concurrent;

namespace TomPIT.BigData.Transactions
{
	internal static class UpdaterPool
	{
		static UpdaterPool()
		{
			Locks = new ConcurrentDictionary<Guid, UpdaterLock>();
		}

		private static ConcurrentDictionary<Guid, UpdaterLock> Locks { get; }

		public static bool Lock(Guid partition)
		{
			if (!Locks.TryGetValue(partition, out UpdaterLock _))
			{
				if (Locks.TryAdd(partition, new UpdaterLock()))
					return true;
			}

			if (Locks.TryGetValue(partition, out UpdaterLock ul))
			{
				lock (ul)
				{
					if (ul.Expired)
					{
						ul.Lock();
						return true;
					}

					return false;
				}
			}

			return false;
		}

		public static void Release(Guid partition)
		{
			if (!Locks.TryGetValue(partition, out UpdaterLock ul))
				return;

			lock (ul)
			{
				ul.Release();
			}
		}
	}
}
