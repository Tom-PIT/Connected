using System;
using System.Collections.Generic;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class DataAudit : IDataAudit
	{
		public DataAudit(IApplicationContext context)
		{
			Context = context;
		}

		private IApplicationContext Context { get; }

		public void Insert(string category, string @event, string primaryKey, string property, string value, string description)
		{
			var user = Context.Services.Identity.IsAuthenticated
				? Context.Services.Identity.User.Token
				: Guid.Empty;

			var request = Context.GetHttpRequest();
			var ip = string.Empty;

			if (request != null)
				ip = request.HttpContext.Connection.RemoteIpAddress.ToString();

			Context.GetServerContext().GetService<IAuditService>().Insert(user, category, @event, primaryKey, ip, new Dictionary<string, string> { { property, value } }, description);
		}

		public void Insert(string category, string @event, string primaryKey, Dictionary<string, string> values, string description)
		{
			var user = Context.Services.Identity.IsAuthenticated
				? Context.Services.Identity.User.Token
				: Guid.Empty;

			var request = Context.GetHttpRequest();
			var ip = string.Empty;

			if (request != null)
				ip = request.HttpContext.Connection.RemoteIpAddress.ToString();

			Context.GetServerContext().GetService<IAuditService>().Insert(user, category, @event, primaryKey, ip, values, description);
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
			return Context.GetServerContext().GetService<IAuditService>().Query(category);
		}

		public List<IAuditDescriptor> Query(string category, string @event)
		{
			return Context.GetServerContext().GetService<IAuditService>().Query(category, @event);
		}

		public List<IAuditDescriptor> Query(string category, string @event, string primaryKey)
		{
			return Context.GetServerContext().GetService<IAuditService>().Query(category, @event, primaryKey);
		}
	}
}
