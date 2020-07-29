using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Caching
{
	internal class EntryEnumerator<T> : IEnumerator<T>
	{
		public EntryEnumerator(ConcurrentDictionary<string, Entry> items)
		{
			Items = items;
			Index = -1;
		}

		private int Count { get { return Items.Count; } }
		private int Index { get; set; }
		private ConcurrentDictionary<string, Entry> Items { get; }

		public T Current => (T)Items.ElementAt(Index).Value.Instance;

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (Index < Count - 1)
			{
				Index++;

				return true;
			}

			return false;
		}

		public void Reset()
		{
			Index = -1;
		}
	}
}
