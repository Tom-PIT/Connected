using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class EventController : SysController
	{
		[HttpPost]
		public Guid Trigger(Guid microService, string name, string callback = null)
		{
			var body = FromBody();

			return DataModel.Events.Insert(microService, name, body, callback);
		}
	}
}
