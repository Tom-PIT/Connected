using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Reflection;
using TomPIT.Security;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareUserDataService : MiddlewareObject, IMiddlewareUserDataService
	{
		public MiddlewareUserDataService(IMiddlewareContext context) : base(context)
		{
		}

		public IUserData Create<A, V>(A primaryKey, V value)
		{
			return Create(primaryKey, value, null);
		}

		public IUserData Create<A, V>(A primaryKey, V value, string topic)
		{
			return new UserData
			{
				PrimaryKey = Serialize(primaryKey),
				Value = Serialize(value),
				Topic = topic
			};
		}

		public List<IUserData> Query(string topic)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Context.Tenant.GetService<IUserDataService>().Query(user, topic);
		}

		public string Select<A>(A primaryKey)
		{
			return Select<string, A>(primaryKey);
		}

		public string Select<A>(A primaryKey, string topic)
		{
			return Select<string, A>(primaryKey, topic);
		}

		public R Select<R, A>(A primaryKey)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Deserialize<R>(Context.Tenant.GetService<IUserDataService>().Select(user, Serialize(primaryKey))?.Value);
		}

		public R Select<R, A>(A primaryKey, string topic)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Deserialize<R>(Context.Tenant.GetService<IUserDataService>().Select(user, Serialize(primaryKey), topic)?.Value);
		}

		public void Update<A, V>(A primaryKey, V value)
		{
			Update(primaryKey, value, null);
		}

		public void Update<A, V>(A primaryKey, V value, string topic)
		{
			Update(new List<IUserData>
			{
				new UserData
				{
					PrimaryKey=Serialize( primaryKey),
					Value=Serialize(value),
					Topic=topic
				}
			});
		}

		public void Update(List<IUserData> data)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			Context.Tenant.GetService<IUserDataService>().Update(user, data);
		}

		private string Serialize(object value)
		{
			if (value == null)
				return null;

			if (value is JValue v)
				return v.Value.ToString();
			else if (value.GetType().IsTypePrimitive())
				return value.ToString();

			return Serializer.Serialize(value);
		}

		private T Deserialize<T>(string value)
		{
			return Serializer.Deserialize<T>(value);
		}
	}
}
