using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Caching
{
	internal class CachingHandlerState
	{
		public IDataCachingHandler Handler { get; set; }
		public bool Initialized { get; set; }
	}
}
