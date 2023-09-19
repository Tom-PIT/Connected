using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.BigData.Partitions
{
    internal class TimeZoneService : SynchronizedClientRepository<ITimeZone, Guid>, ITimeZoneService
    {
        public TimeZoneService(ITenant tenant) : base(tenant, "bigdatatimezone")
        {
        }

        public ITimeZone Select(Guid token)
        {
            return Get(token);
        }

        public ITimeZone Select(string name)
        {
            return Get(f => string.Compare(f.Name, name, true) == 0);
        }

        public ImmutableList<ITimeZone> Query()
        {
            return All();
        }
        protected override void OnInitializing()
        {
            foreach (var timezone in Instance.SysProxy.Management.BigData.QueryTimeZones())
                Set(timezone.Token, timezone, TimeSpan.Zero);
        }

        protected override void OnInvalidate(Guid id)
        {
            if (Instance.SysProxy.Management.BigData.SelectTimeZone(id) is ITimeZone timezone)
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
    }
}
