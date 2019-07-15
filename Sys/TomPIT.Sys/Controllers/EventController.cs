using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class EventController : SysController
	{
		[HttpPost]
		public Guid Trigger()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");
			var name = body.Required<string>("name");
			var callback = body.Optional("callback", string.Empty);
			var args = body.Optional<string>("arguments", null);

			return DataModel.Events.Insert(ms, name, args, callback);
		}
	}
}
