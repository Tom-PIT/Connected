using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Threading
{
	public class LockerContainer<K>:ConcurrentDictionary<K, Locker>
	{
		public Locker Request(K key)
		{
			if (ContainsKey(key))
				return this[key];

			TryAdd(key, new Locker());

			return this[key];
		}
	}
}
