using System.Collections.Generic;

namespace TomPIT.Proxy
{
	public interface IDataCacheController
	{
		void Clear(string key);
		void Invalidate(string key, List<string> ids);
		void Remove(string key, List<string> ids);
	}
}
