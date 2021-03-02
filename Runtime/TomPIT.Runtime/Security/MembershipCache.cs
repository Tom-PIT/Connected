using System;
using System.Collections.Immutable;
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
			var ds = Tenant.Get<ImmutableList<Membership>>(CreateUrl("QueryMembership"));

			foreach (var i in ds)
				Set(GenerateRandomKey(), i, TimeSpan.Zero);
		}

		public ImmutableList<IMembership> QueryForRole(Guid role)
		{
			return Where(f => f.Role == role);
		}
		public ImmutableList<IMembership> Query(Guid user)
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

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl("Security", action);
		}
	}
}
