using System;

namespace TomPIT.Caching
{
	public enum InvalidateBehavior
	{
		RemoveSameInstance = 1,
		KeepSameInstance = 2
	}
	public class CacheEventArgs : EventArgs
	{
		public CacheEventArgs(string id, string key)
		{
			Key = key;
			Id = id;
		}

		public string Id { get; }
		public string Key { get; }

		public InvalidateBehavior InvalidateBehavior { get; set; } = InvalidateBehavior.RemoveSameInstance;
	}
}