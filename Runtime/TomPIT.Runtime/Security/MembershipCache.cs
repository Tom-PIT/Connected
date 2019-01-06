using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class MembershipCache : SynchronizedClientRepository<IMembership, string>
	{
		public MembershipCache(ISysConnection server) : base(server, "membership")
		{
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("Security", "QueryMembership");
			var ds = Connection.Get<List<Membership>>(u).ToList<IMembership>();

			foreach (var i in ds)
				Set(GenerateRandomKey(), i, TimeSpan.Zero);
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
			var u = Connection.CreateUrl("User", "SelectMembership")
				.AddParameter("user", user)
				.AddParameter("role", role);

			var d = Connection.Get<Membership>(u);

			if (d != null)
				Set(GenerateRandomKey(), d, TimeSpan.Zero);
		}

		public IMembership Select(Guid user, Guid role)
		{
			return Get(f => f.User == user && f.Role == role);
		}
	}
}
