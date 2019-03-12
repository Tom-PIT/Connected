using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel.QA
{
	public class Generator
	{
		private Random _random = null;

		public List<DateTime> ToNow(DateTime start, TimeSpan interval)
		{
			var r = new List<DateTime>();

			while (start <= DateTime.UtcNow)
			{
				r.Add(start);

				start = start.Add(interval);
			}

			return r;
		}

		public Random Random
		{
			get
			{
				if (_random == null)
					_random = new Random();

				return _random;
			}
		}
	}
}
