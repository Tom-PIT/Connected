using System;
using System.Collections.Generic;
using TomPIT.Data;

namespace TomPIT.Services.Context
{
	internal class ContextDataAudit : ContextClient, IContextDataAudit
	{
		public ContextDataAudit(IExecutionContext context) : base(context)
		{
		}

		public void Insert(string category, string @event, string primaryKey, string property, string value, string description)
		{
			var user = Context.Services.Identity.IsAuthenticated
				? Context.Services.Identity.User.Token
				: Guid.Empty;

			var ip = string.Empty;

			if (Shell.HttpContext != null)
				ip = Shell.HttpContext.Connection.RemoteIpAddress.ToString();

			Context.Connection().GetService<IAuditService>().Insert(user, category, @event, primaryKey, ip, new Dictionary<string, string> { { property, value } }, description);
		}

		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description)
		{
			var user = Context.Services.Identity.IsAuthenticated
				? Context.Services.Identity.User.Token
				: Guid.Empty;

			var ip = string.Empty;

			if (Shell.HttpContext != null)
				ip = Shell.HttpContext.Connection.RemoteIpAddress.ToString();

			Context.Connection().GetService<IAuditService>().Insert(user, category, @event, primaryKey, ip, values, description);
		}

		public void Insert(string category, string @event, string primaryKey, string property, string value)
		{
			Insert(category, @event, primaryKey, property, value, null);
		}

		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values)
		{
			Insert(category, @event, primaryKey, values, null);
		}

		public List<IAuditDescriptor> Query(string category)
		{
			return Context.Connection().GetService<IAuditService>().Query(category);
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			return Context.Connection().GetService<IAuditService>().Query(category, @event);
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return Context.Connection().GetService<IAuditService>().Query(category, @event, primaryKey);
		}
	}
}
