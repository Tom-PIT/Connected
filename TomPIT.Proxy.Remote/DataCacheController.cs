using System.Collections.Generic;

namespace TomPIT.Proxy.Remote
{
	internal class DataCacheController : IDataCacheController
	{
		private const string Controller = "DataCache";
		public void Clear(string key)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Clear"), new
			{
				key
			});
		}

		public void Invalidate(string key, List<string> ids)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Invalidate"), new
			{
				key,
				ids
			});
		}

		public void Remove(string key, List<string> ids)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Remove"), new
			{
				key,
				ids
			});
		}
	}
}
