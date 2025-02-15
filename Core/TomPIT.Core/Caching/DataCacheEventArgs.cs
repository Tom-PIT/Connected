﻿using System;
using System.Collections.Generic;

namespace TomPIT.Caching
{
	public class DataCacheEventArgs : EventArgs
	{
		public DataCacheEventArgs()
		{

		}
		public DataCacheEventArgs(string key, List<string> ids)
		{
			Key = key;
			Ids = ids;
		}

		public DataCacheEventArgs(string key)
		{
			Key = key;
		}

		public List<string> Ids { get; set; }
		public string Key { get; set; }
	}
}