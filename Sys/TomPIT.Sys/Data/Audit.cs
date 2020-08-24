using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class Audit
	{
		public void Insert(Guid user, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description)
		{
			if (values.Count == 0)
				return;

			var items = Query(category, null, primaryKey);
			var newItems = new Dictionary<string, string>();

			if (items.Count > 0)
			{
				foreach (var i in values)
				{
					var last = items.OrderByDescending(f => f.Created).FirstOrDefault(f => string.Compare(f.Property, i.Key, true) == 0);

					if (last != null && string.Compare(last.Value, i.Value, false) == 0)
						continue;

					newItems.Add(i.Key, i.Value);
				}
			}
			else
				newItems = values;

			IUser u = null;

			if (user != Guid.Empty)
			{
				u = DataModel.Users.Select(user);

				if (u == null)
					throw new SysException(SR.ErrUserNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Data.Audit.Insert(u, DateTime.UtcNow, category, @event, primaryKey, ip, newItems, description);
		}

		public List<IAuditDescriptor> Query(string category)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Data.Audit.Query(category);
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Data.Audit.Query(category, @event);
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Data.Audit.Query(category, @event, primaryKey);
		}
	}
}
