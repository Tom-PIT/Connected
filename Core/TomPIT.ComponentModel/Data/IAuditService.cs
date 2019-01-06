using System;
using System.Collections.Generic;
using TomPIT.Data;

namespace TomPIT.Data
{
	public interface IAuditService
	{
		void Insert(Guid user, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description);
		List<IAuditDescriptor> Query(string category);
		List<IAuditDescriptor> Query(string category, string @event);
		List<IAuditDescriptor> Query(string category, string @event, string primaryKey);
	}
}
