using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Data;

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
