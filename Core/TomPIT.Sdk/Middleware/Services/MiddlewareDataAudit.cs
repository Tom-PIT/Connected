using System;
using System.Collections.Generic;
using TomPIT.Data;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDataAudit : MiddlewareObject, IMiddlewareDataAudit
	{
		public MiddlewareDataAudit(IMiddlewareContext context) : base(context)
		{
		}

		public void Insert(string category, string @event, string primaryKey, string property, string value, string description, Guid user)
		{
			var ip = string.Empty;

			if (Shell.HttpContext != null)
				ip = Shell.HttpContext.Connection.RemoteIpAddress.ToString();

			Context.Tenant.GetService<IAuditService>().Insert(user, category, @event, primaryKey, ip, new Dictionary<string, string> { { property, value } }, description);
		}
		public void Insert(string category, string @event, string primaryKey, string property, string value, string description)
		{
			var user = Context.Services.Identity.IsAuthenticated
				? Context.Services.Identity.User.Token
				: Guid.Empty;

			Insert(category, @event, primaryKey, property, value, description, user);
		}

		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description, Guid user)
		{
			var ip = string.Empty;

			if (Shell.HttpContext != null)
				ip = Shell.HttpContext.Connection.RemoteIpAddress.ToString();

			Context.Tenant.GetService<IAuditService>().Insert(user, category, @event, primaryKey, ip, values, description);
		}
		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description)
		{
			var user = Context.Services.Identity.IsAuthenticated
				? Context.Services.Identity.User.Token
				: Guid.Empty;

			Insert(category, @event, primaryKey, values, description, user);
		}

		public void Insert(string category, string @event, string primaryKey, string property, string value, Guid user)
		{
			Insert(category, @event, primaryKey, property, value, null, user);
		}
		public void Insert(string category, string @event, string primaryKey, string property, string value)
		{
			Insert(category, @event, primaryKey, property, value, null);
		}

		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, Guid user)
		{
			Insert(category, @event, primaryKey, values, null, user);
		}
		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values)
		{
			Insert(category, @event, primaryKey, values, null);
		}

		public List<IAuditDescriptor> Query(string category)
		{
			return Context.Tenant.GetService<IAuditService>().Query(category);
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			return Context.Tenant.GetService<IAuditService>().Query(category, @event);
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return Context.Tenant.GetService<IAuditService>().Query(category, @event, primaryKey);
		}
	}
}
