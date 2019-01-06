using System.Collections.Generic;
using TomPIT.Data;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface IDataAudit
	{
		void Insert(string category, string @event, string primaryKey, string property, string value, string description);
		void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description);
		void Insert(string category, string @event, string primaryKey, string property, string value);
		void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values);

		List<IAuditDescriptor> Query(string category);
		List<IAuditDescriptor> Query(string category, string @event);
		List<IAuditDescriptor> Query(string category, string @event, string primaryKey);
	}
}
