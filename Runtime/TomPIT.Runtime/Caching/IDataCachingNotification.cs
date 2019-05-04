using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Caching
{
	internal interface IDataCachingNotification
	{
		void NotifyClear(DataCacheEventArgs e);
		void NotifyInvalidate(DataCacheEventArgs e);
		void NotifyRemove(DataCacheEventArgs e);
	}
}
