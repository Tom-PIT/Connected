using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Data;
using DataModel = TomPIT.Sys.Model.DataModel;

namespace TomPIT.Proxy.Local
{
	internal class AuditController : IAuditController
	{
		public void Insert(Guid user, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description)
		{
			DataModel.Audit.Insert(user, category, @event, primaryKey, ip, values, description);
		}

		public ImmutableList<IAuditDescriptor> Query(string category)
		{
			return DataModel.Audit.Query(category).ToImmutableList();
		}

		public ImmutableList<IAuditDescriptor> Query(string category, string @event)
		{
			return DataModel.Audit.Query(category, @event).ToImmutableList();
		}

		public ImmutableList<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return DataModel.Audit.Query(category, @event, primaryKey).ToImmutableList();
		}
	}
}
