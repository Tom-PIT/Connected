using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Security.Authentication
{
	internal class AuthenticationTokensCache : SynchronizedClientRepository<IAuthenticationToken, Guid>
	{
		public object FirstOrDefault { get; internal set; }

		public AuthenticationTokensCache(ITenant tenant) : base(tenant, "authtoken")
		{
		}

		protected override void OnInitializing()
		{
			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
			{
				var u = Tenant.CreateUrl("Security", "QueryAllAuthenticationTokens");
				var ds = Tenant.Post<List<AuthenticationToken>>(u).ToList<IAuthenticationToken>();

				foreach (var i in ds)
					Set(i.Token, i, TimeSpan.Zero);
			}
			else
			{
				var u = Tenant.CreateUrl("Security", "QueryAuthenticationTokens");
				var a = new JArray();
				var e = new JObject
				{
					{"data", a }
				};

				foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
					a.Add(i);

				var ds = Tenant.Post<List<AuthenticationToken>>(u, e).ToList<IAuthenticationToken>();

				foreach (var i in ds)
					Set(i.Token, i, TimeSpan.Zero);
			}
		}

		protected override void OnInvalidate(Guid token)
		{
			var u = Tenant.CreateUrl("Security", "SelectAuthenticationToken")
				.AddParameter("token", token);

			var d = Tenant.Get<AuthenticationToken>(u);

			if (d != null)
				Set(d.Token, d, TimeSpan.Zero);
		}

		public List<IAuthenticationToken> Query(Guid resourceGroup)
		{
			return Where(f => f.ResourceGroup == resourceGroup);
		}

		public IAuthenticationToken Select(string key)
		{
			return Get(f => string.Compare(f.Key, key, false) == 0);
		}

		public void NotifyChanged(Guid id)
		{
			Refresh(id);
		}

		public void NotifyRemoved(Guid id)
		{
			Remove(id);
		}
	}
}
