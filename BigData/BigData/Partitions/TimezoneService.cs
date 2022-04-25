using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.BigData.Partitions
{
	internal class TimezoneService : SynchronizedClientRepository<ITimezone, Guid>, ITimezoneService
	{
		private const string DefaultController = "BigDataManagement";
		public TimezoneService(ITenant tenant) : base(tenant, "bigdatatimezone")
		{
		}

		public ITimezone Select(Guid token)
		{
			return Get(token);
		}

		public ITimezone Select(string name)
		{
			return Get(f => string.Compare(f.Name, name, true) == 0);
		}

		public ImmutableList<ITimezone> Query()
		{
			return All();
		}
		protected override void OnInitializing()
		{
			foreach (var timezone in Tenant.Get<List<Timezone>>(CreateUrl("QueryTimezones")))
				Set(timezone.Token, timezone, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Tenant.Post<Timezone>(CreateUrl("SelectTimezone"), new { token = id }) is ITimezone timezone)
				Set(timezone.Token, timezone, TimeSpan.Zero);
		}

		public void NotifyChanged(Guid token)
		{
			Refresh(token);
		}

		public void NotifyRemoved(Guid token)
		{
			Remove(token);
		}

		private string CreateUrl(string action)
		{
			return Tenant.CreateUrl(DefaultController, action);
		}
	}
}
