using System.Collections.Generic;
using TomPIT.Analytics;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareMruService
	{
		void Modify(int type, string primaryKey, List<string> tags);
		List<IMru> Query(List<string> tags);
	}
}
