using System;
using System.Collections.Immutable;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.BigData
{
	internal class BigDataTimezonesModel : SynchronizedRepository<ITimezone, Guid>
	{
		public BigDataTimezonesModel(IMemoryCache container) : base(container, "bigdatatimezones")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Timezones.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Timezones.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public ITimezone Select(Guid token)
		{
			return Get(token);
		}

		public ImmutableList<ITimezone> Query()
		{
			return All();
		}

		public Guid Insert(string name, int offset)
		{
			var id = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.BigData.Timezones.Insert(id, name, offset);

			Refresh(id);
			BigDataNotifications.TimezoneAdded(id);

			return id;
		}

		public void Update(Guid token, string name, int offset)
		{
			var timezone = Select(token);

			if (timezone is null)
				throw new SysException(SR.ErrBigDataTimezoneNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Timezones.Update(timezone, name, offset);

			Refresh(timezone.Token);
			BigDataNotifications.TimezoneChanged(timezone.Token);
		}

		public void Delete(Guid token)
		{
			var timezone = Select(token);

			if (timezone is null)
				throw new SysException(SR.ErrBigDataTimezoneNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Timezones.Delete(timezone);

			Refresh(timezone.Token);
			BigDataNotifications.TimezoneRemoved(timezone.Token);
		}
	}
}
