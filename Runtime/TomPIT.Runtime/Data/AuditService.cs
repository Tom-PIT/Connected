using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	internal class AuditService : TenantObject, IAuditService
	{
		public AuditService(ITenant tenant) : base(tenant)
		{

		}

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

			var url = Tenant.CreateUrl("Audit", "Insert");
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

			Tenant.Post(url, e);
		}

		public List<IAuditDescriptor> Query(string category)
		{
			var url = Tenant.CreateUrl("Audit", "QueryByCategory")
				.AddParameter("category", category);

			return Tenant.Get<List<AuditDescriptor>>(url).ToList<IAuditDescriptor>();
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			var url = Tenant.CreateUrl("Audit", "QueryByEvent")
				.AddParameter("category", category)
				.AddParameter("@event", @event);

			return Tenant.Get<List<AuditDescriptor>>(url).ToList<IAuditDescriptor>();
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			var url = Tenant.CreateUrl("Audit", "Query")
				.AddParameter("category", category)
				.AddParameter("@event", @event)
				.AddParameter("primaryKey", primaryKey);

			return Tenant.Get<List<AuditDescriptor>>(url).ToList<IAuditDescriptor>();
		}
	}
}
