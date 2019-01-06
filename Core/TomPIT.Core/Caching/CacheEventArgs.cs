using System;

namespace TomPIT.Caching
{
	public class CacheEventArgs : EventArgs
	{
		public CacheEventArgs(string id, string key)
		{
			Key = key;
			Id = id;
		}

		public string Id { get; }
		public string Key { get; }
	}
}