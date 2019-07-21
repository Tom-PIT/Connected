using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;
using TomPIT.Search.Catalogs;

namespace TomPIT.Search.Controllers
{
	[AllowAnonymous]
	public class SearchController : ServerController
	{
		[HttpPost]
		public ISearchResults Search()
		{
			var body = FromBody<SearchOptions>();
			var transaction = new SearchTransaction();

			transaction.Search(body);

			return transaction.Results;
		}
	}
}
