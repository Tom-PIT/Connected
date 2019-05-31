using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class QueueController : SysController
	{
		[HttpPost]
		public void Enqueue()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var args = body.Optional<JObject>("arguments", null);
			var expire = body.Optional("expire", TimeSpan.FromDays(2));
			var nextVisible = body.Optional("nextVisible", TimeSpan.Zero);

			var message = new JObject
			{
				{"component", component }
			};

			if (args != null)
				message.Add("arguments", args);

			DataModel.Queue.Enqueue(JsonConvert.SerializeObject(message), expire, nextVisible);
		}
	}
}