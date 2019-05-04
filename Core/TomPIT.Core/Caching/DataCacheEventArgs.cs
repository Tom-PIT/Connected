using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Caching
{
	public class DataCacheEventArgs : EventArgs
	{
		public DataCacheEventArgs(string key, List<string> ids)
		{
			Key = key;
			Ids = ids;
		}

		public DataCacheEventArgs(string key)
		{
			Key = key;
		}

		public List<string> Ids { get; }
		public string Key { get; }
	}
}