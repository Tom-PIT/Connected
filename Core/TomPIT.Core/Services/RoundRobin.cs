using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TomPIT.Services
{
	public class RoundRobin
	{
		private ConcurrentQueue<Guid> _items = new ConcurrentQueue<Guid>();
		private object _sync = new object();

		public void Register(Guid value)
		{
			_items.Enqueue(value);
		}

		public void Remove(Guid value)
		{
			lock (_items)
			{
				var values = _items.ToArray();

				if (values == null || values.Length == 0)
					return;

				var newArray = new List<Guid>();

				foreach (var i in values)
				{
					if (i == value)
						continue;

					newArray.Add(i);
				}

				while (!_items.IsEmpty)
					_items.TryDequeue(out Guid tmp);

				foreach (var i in newArray)
					_items.Enqueue(i);
			}
		}
		public Guid Next()
		{
			if (_items.IsEmpty)
				return Guid.Empty;

			var r = TryFirst();

			if (r != Guid.Empty)
				return r;

			lock (_sync)
			{
				if (_items.TryDequeue(out r))
					_items.Enqueue(r);
			}

			return r;
		}

		private Guid TryFirst()
		{
			if (_items.Count == 1)
			{
				if (_items.TryPeek(out Guid r))
					return r;
			}

			return Guid.Empty;
		}
	}
}