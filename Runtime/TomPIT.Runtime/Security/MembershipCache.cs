using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	internal class MembershipCache : SynchronizedClientRepository<IMembership, string>
	{
		public MembershipCache(ITenant tenant) : base(tenant, "membership")
		{
		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("Security", "QueryMembership");
			var ds = Tenant.Get<List<Membership>>(u).ToList<IMembership>();

			foreach (var i in ds)
				Set(GenerateRandomKey(), i, TimeSpan.Zero);
		}

		public List<IMembership> QueryForRole(Guid role)
		{
			return Where(f => f.Role == role);
		}
		public List<IMembership> Query(Guid user)
		{
			return Where(f => f.User == user);
		}

		public void Remove(Guid user, Guid role)
		{
			Remove(f => f.User == user && f.Role == role);
		}

		public void Add(Guid user, Guid role)
		{
			var u = Tenant.CreateUrl("User", "SelectMembership")
				.AddParameter("user", user)
				.AddParameter("role", role);

			var d = Tenant.Get<Membership>(u);

			if (d != null)
				Set(GenerateRandomKey(), d, TimeSpan.Zero);
		}

		public IMembership Select(Guid user, Guid role)
		{
			return Get(f => f.User == user && f.Role == role);
		}
	}
}
