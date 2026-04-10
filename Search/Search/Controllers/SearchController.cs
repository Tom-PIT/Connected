using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;
using TomPIT.Middleware;
using TomPIT.Search;

namespace TomPIT.Search.Controllers
{
	[AllowAnonymous]
	public class SearchController : ServerController
	{
		[HttpPost]
		public ISearchResults Search()
		{
			var body = FromBody<SearchOptions>();

			return MiddlewareDescriptor.Current.Tenant.GetService<ISearchNodeProxy>().Search(body);
		}
	}
}
