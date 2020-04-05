using System;
using System.Collections.Generic;
using TomPIT.Data;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareDataAudit
	{
		void Insert(string category, string @event, string primaryKey, string property, string value, string description);
		void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description);
		void Insert(string category, string @event, string primaryKey, string property, string value);
		void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values);

		void Insert(string category, string @event, string primaryKey, string property, string value, string description, Guid user);
		void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description, Guid user);
		void Insert(string category, string @event, string primaryKey, string property, string value, Guid user);
		void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, Guid user);

		List<IAuditDescriptor> Query(string category);
		List<IAuditDescriptor> Query(string category, string @event);
		List<IAuditDescriptor> Query(string category, string @event, string primaryKey);
	}
}
