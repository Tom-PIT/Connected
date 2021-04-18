using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class SearchController : SysController
	{
		[HttpPost]
		public void Index()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");
			var name = body.Required<string>("catalog");
			var args = body.Optional<string>("arguments", null);

			DataModel.Search.Insert(ms, name, args);
		}
	}
}
