using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Data
{
	internal class AuditService : TenantObject, IAuditService
	{
		public AuditService(ITenant tenant) : base(tenant)
		{

		}

		public void Insert(Guid user, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description)
		{
			Instance.SysProxy.Audit.Insert(user, category, @event, primaryKey, ip, values, description);
		}

		public List<IAuditDescriptor> Query(string category)
		{
			return Instance.SysProxy.Audit.Query(category).ToList();
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			return Instance.SysProxy.Audit.Query(category, @event).ToList();
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return Instance.SysProxy.Audit.Query(category, @event, primaryKey).ToList();
		}
	}
}
