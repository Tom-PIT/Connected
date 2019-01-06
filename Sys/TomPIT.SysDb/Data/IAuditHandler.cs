using System;
using System.Collections.Generic;
using TomPIT.Data;
using TomPIT.Security;

namespace TomPIT.SysDb.Data
{
	public interface IAuditHandler
	{
		void Insert(IUser user, DateTime created, string category, string @event, string primaryKey, string ip, Dictionary<string, string> values, string description);
		List<IAuditDescriptor> Query(string category);
		List<IAuditDescriptor> Query(string category, string @event);
		List<IAuditDescriptor> Query(string category, string @event, string primaryKey);
	}
}
