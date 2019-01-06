using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class AuditController : SysController
	{
		[HttpPost]
		public void Insert()
		{
			var body = FromBody();

			var user = body.Optional<Guid>("user", Guid.Empty);
			var primaryKey = body.Required<string>("primaryKey");
			var category = body.Required<string>("category");
			var @event = body.Required<string>("event");
			var description = body.Optional<string>("description", string.Empty);
			var ip = body.Optional<string>("ip", string.Empty);
			var values = (JArray)body["values"];
			var d = new Dictionary<string, string>();

			foreach (var i in values.Children())
			{
				if (i is JObject jo)
				{
					foreach (var j in jo.Properties())
						d.Add(j.Name, (string)j.Value);
				}
			}

			DataModel.Audit.Insert(user, category, @event, primaryKey, ip, d, description);
		}

		[HttpGet]
		public List<IAuditDescriptor> QueryByCategory(string category)
		{
			return DataModel.Audit.Query(category);
		}

		[HttpGet]
		public List<IAuditDescriptor> QueryByEvent(string category, string @event)
		{
			return DataModel.Audit.Query(category, @event);
		}

		[HttpGet]
		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return DataModel.Audit.Query(category, @event, primaryKey);
		}
	}
}
