using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class MembershipCache : SynchronizedClientRepository<IMembership, string>
	{
		public MembershipCache(ITenant tenant) : base(tenant, "membership")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Instance.SysProxy.Security.QueryMembership();

			foreach (var i in ds)
				Set(GenerateKey(i), i, TimeSpan.Zero);
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
			var d = Instance.SysProxy.Users.SelectMembership(user, role);

			if (d is not null)
				Set(GenerateKey(d), d, TimeSpan.Zero);
		}

		public IMembership Select(Guid user, Guid role)
		{
			return Get(f => f.User == user && f.Role == role);
		}

		private static string GenerateKey(IMembership membership)
		{
			return $"{membership.Role}{membership.User}";
		}
	}
}
