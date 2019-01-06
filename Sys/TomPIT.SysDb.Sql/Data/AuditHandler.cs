using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data;
using TomPIT.Data.Sql;
using TomPIT.Security;
using TomPIT.SysDb.Data;

namespace TomPIT.SysDb.Sql.Data
{
	internal class AuditHandler : IAuditHandler
	{
		public void Insert(IUser user, DateTime created, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description)
		{
			var w = new Writer("tompit.audit_ins");

			w.CreateParameter("@user", user == null ? Guid.Empty : user.Token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@primary_key", primaryKey);
			w.CreateParameter("@category", category);
			w.CreateParameter("@event", @event);
			w.CreateParameter("@ip", ip, true);
			w.CreateParameter("@description", description, true);
			w.CreateParameter("@property", DBNull.Value);
			w.CreateParameter("@value", DBNull.Value);
			w.CreateParameter("@identifier", Guid.Empty);

			w.Prepare();

			try
			{
				foreach (var i in values)
				{
					w.ModifyParameter("@property", i.Key);
					w.ModifyParameter("@value", i.Value);
					w.ModifyParameter("@identifier", Guid.NewGuid());

					w.Execute();
				}
			}
			finally
			{
				w.Complete();
			}
		}

		public List<IAuditDescriptor> Query(string category)
		{
			var r = new Reader<AuditDescriptor>("tompit.audit_que");

			r.CreateParameter("@category", category);

			return r.Execute().ToList<IAuditDescriptor>();
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			var r = new Reader<AuditDescriptor>("tompit.audit_que");

			r.CreateParameter("@category", category);
			r.CreateParameter("@event", @event);

			return r.Execute().ToList<IAuditDescriptor>();
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			var r = new Reader<AuditDescriptor>("tompit.audit_que");

			r.CreateParameter("@category", category);
			r.CreateParameter("@event", @event, true);
			r.CreateParameter("@primary_key", primaryKey);

			return r.Execute().ToList<IAuditDescriptor>();

		}
	}
}
