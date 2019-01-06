using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Net;

namespace TomPIT.ComponentModel.Data
{
	internal class AuditService : IAuditService
	{
		public AuditService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

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

			var url = Server.CreateUrl("Audit", "Insert");
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

			Server.Connection.Post(url, e);
		}

		public List<IAuditDescriptor> Query(string category)
		{
			var url = Server.CreateUrl("Audit", "QueryByCategory")
				.AddParameter("category", category);

			return Server.Connection.Get<List<AuditDescriptor>>(url).ToList<IAuditDescriptor>();
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			var url = Server.CreateUrl("Audit", "QueryByEvent")
				.AddParameter("category", category)
				.AddParameter("@event", @event);

			return Server.Connection.Get<List<AuditDescriptor>>(url).ToList<IAuditDescriptor>();
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			var url = Server.CreateUrl("Audit", "Query")
				.AddParameter("category", category)
				.AddParameter("@event", @event)
				.AddParameter("primary_key", primaryKey);

			return Server.Connection.Get<List<AuditDescriptor>>(url).ToList<IAuditDescriptor>();
		}
	}
}
