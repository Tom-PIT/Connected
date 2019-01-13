using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class AuthenticationTokenManagementService : IAuthenticationTokenManagementService
	{
		public AuthenticationTokenManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid token)
		{
			var u = Connection.CreateUrl("SecurityManagement", "DeleteAuthenticationToken");
			var e = new JObject
			{
				{"token", token }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				n.NotifyAuthenticationTokenRemoved(this, new AuthenticationTokenEventArgs(token));
		}

		public Guid Insert(Guid resourceGroup, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status,
			DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			var u = Connection.CreateUrl("SecurityManagement", "InsertAuthenticationToken");
			var e = new JObject
			{
				{"resourceGroup", resourceGroup },
				{"user", user },
				{"key", key },
				{"claims", claims.ToString() },
				{"status", status.ToString() },
				{"validFrom", validFrom },
				{"validTo", validTo },
				{"startTime", startTime },
				{"endTime", endTime },
				{"ipRestrictions", ipRestrictions },
				{"name", name },
				{"description", description }
			};

			var id = Connection.Post<Guid>(u, e);

			if (Connection.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				n.NotifyAuthenticationTokenChanged(this, new AuthenticationTokenEventArgs(id));

			return id;
		}

		public List<IAuthenticationToken> Query(string resourceGroup)
		{
			var u = Connection.CreateUrl("Security", "QueryAuthenticationTokens");
			var a = new JArray();
			var e = new JObject
				{
					{"data", a }
				};

			a.Add(resourceGroup);

			return Connection.Post<List<AuthenticationToken>>(u, e).ToList<IAuthenticationToken>();
		}

		public IAuthenticationToken Select(Guid token)
		{
			var u = Connection.CreateUrl("Security", "SelectAuthenticationToken")
				.AddParameter("token", token);

			return Connection.Get<AuthenticationToken>(u);
		}

		public void Update(Guid token, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			var u = Connection.CreateUrl("SecurityManagement", "UpdateAuthenticationToken");
			var e = new JObject
			{
				{"token", token },
				{"user", user },
				{"key", key },
				{"claims", claims.ToString() },
				{"status", status.ToString() },
				{"validFrom", validFrom },
				{"validTo", validTo },
				{"startTime", startTime },
				{"endTime", endTime },
				{"ipRestrictions", ipRestrictions },
				{"name", name },
				{"description", description }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				n.NotifyAuthenticationTokenChanged(this, new AuthenticationTokenEventArgs(token));
		}
	}
}
