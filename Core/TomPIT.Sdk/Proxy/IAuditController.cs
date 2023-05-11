using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Data;

namespace TomPIT.Proxy
{
	public interface IAuditController
	{
		void Insert(Guid user, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description);
		ImmutableList<IAuditDescriptor> Query(string category);
		ImmutableList<IAuditDescriptor> Query(string category, string @event);
		ImmutableList<IAuditDescriptor> Query(string category, string @event, string primaryKey);
	}
}
