using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Caching
{
	public interface IDataCachingHandler
	{
		void Invalidate(string id);
		void Initialize();
	}
}
