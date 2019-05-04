using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Controllers
{
	public class DataCacheController : SysController
	{
		[HttpPost]
		public void Clear()
		{
			var body = FromBody();

			DataCachingNotifications.Clear(body.Required<string>("key"));
		}

		[HttpPost]
		public void Invalidate()
		{
			var body = FromBody();
			var key = body.Required<string>("key");
			var a = body.Required<JArray>("ids");
			var items = new List<string>();

			foreach (JValue id in a)
				items.Add(id.Value<string>());

			DataCachingNotifications.Invalidate(key, items);
		}

		[HttpPost]
		public void Remove()
		{
			var body = FromBody();
			var key = body.Required<string>("key");
			var a = body.Required<JArray>("ids");
			var items = new List<string>();

			foreach (JValue id in a)
				items.Add(id.Value<string>());

			DataCachingNotifications.Remove(key, items);
		}
	}
}
