using System;

namespace TomPIT.Caching
{
	public class EntryOptions
	{
		public string Key { get; set; }
		public string KeyProperty { get; set; }
		public TimeSpan Duration { get; set; }
		public bool SlidingExpiration { get; set; }
		public bool AllowNull { get; set; }
		public CacheScope Scope { get; set; }

		public EntryOptions()
		{
			Duration = TimeSpan.FromMinutes(5);
			SlidingExpiration = true;
		}
	}
}
