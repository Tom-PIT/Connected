using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Data;

namespace TomPIT.Proxy.Remote
{
	internal class AuditController : IAuditController
	{
		private const string Controller = "Audit";
		public void Insert(Guid user, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description)
		{
			var vals = new JArray();

			foreach (var i in values)
			{
				vals.Add(new JObject
				{
					{  i.Key, i.Value}
				});
			}

			var url = Connection.CreateUrl(Controller, "Insert");
			var e = new JObject
			{
				{"user",user },
				{"category",category },
				{"event",@event},
				{"primaryKey",primaryKey },
				{"ip",ip },
				{"description",description },
				{"values",vals },
			};

			Connection.Post(url, e);
		}

		public ImmutableList<IAuditDescriptor> Query(string category)
		{
			var url = Connection.CreateUrl(Controller, "QueryByCategory")
				.AddParameter("category", category);

			return Connection.Get<List<AuditDescriptor>>(url).ToImmutableList<IAuditDescriptor>();
		}

		public ImmutableList<IAuditDescriptor> Query(string category, string @event)
		{
			var url = Connection.CreateUrl("Audit", "QueryByEvent")
				.AddParameter("category", category)
				.AddParameter("@event", @event);

			return Connection.Get<List<AuditDescriptor>>(url).ToImmutableList<IAuditDescriptor>();
		}

		public ImmutableList<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			var url = Connection.CreateUrl("Audit", "Query")
				.AddParameter("category", category)
				.AddParameter("@event", @event)
				.AddParameter("primaryKey", primaryKey);

			return Connection.Get<List<AuditDescriptor>>(url).ToImmutableList<IAuditDescriptor>();
		}
	}
}
