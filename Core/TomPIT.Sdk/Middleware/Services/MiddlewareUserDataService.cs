using System;
using System.Collections.Generic;
using System.Globalization;
using TomPIT.Data;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareUserDataService : MiddlewareObject, IMiddlewareUserDataService
	{
		public MiddlewareUserDataService(IMiddlewareContext context) : base(context)
		{
		}

		public IUserData Create(string primaryKey, object value)
		{
			return Create(primaryKey, value, null);
		}

		public IUserData Create(string primaryKey, object value, string topic)
		{
			return new UserData
			{
				PrimaryKey = primaryKey,
				Value = Types.Convert<string>(value, CultureInfo.InvariantCulture),
				Topic = topic
			};
		}

		public List<IUserData> Query(string topic)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Context.Tenant.GetService<IUserDataService>().Query(user, topic);
		}

		public T Select<T>(string primaryKey, string topic)
		{
			var r = Select(primaryKey, topic);

			if (string.IsNullOrWhiteSpace(r))
				return default(T);

			return Types.Convert<T>(r, CultureInfo.InvariantCulture);
		}

		public T Select<T>(string primaryKey)
		{
			var r = Select(primaryKey);

			if (string.IsNullOrWhiteSpace(r))
				return default(T);

			return Types.Convert<T>(r, CultureInfo.InvariantCulture);
		}

		public string Select(string primaryKey)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Context.Tenant.GetService<IUserDataService>().Select(user, primaryKey)?.Value;
		}

		public string Select(string primaryKey, string topic)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Context.Tenant.GetService<IUserDataService>().Select(user, primaryKey, topic)?.Value;
		}

		public void Update(string primaryKey, object value)
		{
			Update(primaryKey, value, null);
		}

		public void Update(string primaryKey, object value, string topic)
		{
			Update(new List<IUserData>
			{
				new UserData
				{
					PrimaryKey=primaryKey,
					Value=Types.Convert<string>(value, CultureInfo.InvariantCulture),
					Topic=topic
				}
			});
		}

		public void Update(List<IUserData> data)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			Context.Tenant.GetService<IUserDataService>().Update(user, data);
		}
	}
}
