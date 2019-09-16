using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	internal class AuthenticationTokenManagementService : TenantObject, IAuthenticationTokenManagementService
	{
		public AuthenticationTokenManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid token)
		{
			var u = Tenant.CreateUrl("SecurityManagement", "DeleteAuthenticationToken");
			var e = new JObject
			{
				{"token", token }
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				n.NotifyAuthenticationTokenRemoved(this, new AuthenticationTokenEventArgs(token));
		}

		public Guid Insert(Guid resourceGroup, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status,
			DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			var u = Tenant.CreateUrl("SecurityManagement", "InsertAuthenticationToken");
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

			var id = Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				n.NotifyAuthenticationTokenChanged(this, new AuthenticationTokenEventArgs(id));

			return id;
		}

		public List<IAuthenticationToken> Query(string resourceGroup)
		{
			var u = Tenant.CreateUrl("Security", "QueryAuthenticationTokens");
			var a = new JArray();
			var e = new JObject
				{
					{"data", a }
				};

			a.Add(resourceGroup);

			return Tenant.Post<List<AuthenticationToken>>(u, e).ToList<IAuthenticationToken>();
		}

		public IAuthenticationToken Select(Guid token)
		{
			var u = Tenant.CreateUrl("Security", "SelectAuthenticationToken")
				.AddParameter("token", token);

			return Tenant.Get<AuthenticationToken>(u);
		}

		public void Update(Guid token, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			var u = Tenant.CreateUrl("SecurityManagement", "UpdateAuthenticationToken");
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

			Tenant.Post(u, e);

			if (Tenant.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				n.NotifyAuthenticationTokenChanged(this, new AuthenticationTokenEventArgs(token));
		}
	}
}
