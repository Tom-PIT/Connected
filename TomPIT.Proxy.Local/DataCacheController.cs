using System.Collections.Generic;
using TomPIT.Sys.Notifications;

namespace TomPIT.Proxy.Local
{
	internal class DataCacheController : IDataCacheController
	{
		public void Clear(string key)
		{
			DataCachingNotifications.Clear(key);
		}

		public void Invalidate(string key, List<string> ids)
		{
			DataCachingNotifications.Invalidate(key, ids);
		}

		public void Remove(string key, List<string> ids)
		{
			DataCachingNotifications.Remove(key, ids);
		}
	}
}
