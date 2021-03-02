using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Cdn;

namespace TomPIT.Sys.Controllers
{
	public class QueueController : SysController
	{
		[HttpPost]
		public void Enqueue()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var worker = body.Required<string>("worker");
			var args = body.Optional("arguments", string.Empty);
			var expire = body.Optional("expire", TimeSpan.FromDays(2));
			var nextVisible = body.Optional("nextVisible", TimeSpan.Zero);
			var bufferKey = body.Required<string>("bufferKey");
			var message = new JObject
			{
				{"component", component },
				{"worker", worker}
			};

			if (args != null)
				message.Add("arguments", args);

			DataModel.Queue.Enqueue(QueueingModel.Queue, JsonConvert.SerializeObject(message), bufferKey, expire, nextVisible, Storage.QueueScope.Content);
		}
	}
}