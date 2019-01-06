using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	public class EnvironmentUnits : SynchronizedRepository<IEnvironmentUnit, Guid>
	{
		public EnvironmentUnits(IMemoryCache container) : base(container, "environmentunit")
		{
		}

		public IEnvironmentUnit GetByToken(Guid token)
		{
			return Get(token,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					return Shell.GetService<IDatabaseService>().Proxy.Environment.SelectEnvironmentUnit(token);
				});
		}

		public List<IEnvironmentUnit> Query()
		{
			return All();
		}

		public List<IEnvironmentUnit> Where(Guid parent)
		{
			return Where(f => f.Parent == parent);
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Environment.QueryEnvironmentUnits();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectEnvironmentUnit(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public Guid Insert(string name, Guid parent, int ordinal)
		{
			var token = Guid.NewGuid();

			IEnvironmentUnit p = null;

			if (parent != Guid.Empty)
			{
				p = GetByToken(parent);

				if (p == null)
					throw new SysException(SR.ErrEnvironmentUnitNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Environment.InsertEnvironmentUnit(token, name, p, ordinal);

			Refresh(token);

			NotificationHubs.EnvironmentUnitChanged(token);

			return token;
		}

		public void Update(Guid token, string name, Guid parent, int ordinal)
		{
			var target = GetByToken(token);

			if (target == null)
				throw new SysException(SR.ErrEnvironmentUnitNotFound);

			IEnvironmentUnit p = null;

			if (parent != Guid.Empty)
			{
				p = GetByToken(parent);

				if (p == null)
					throw new SysException(SR.ErrEnvironmentUnitNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Environment.UpdateEnvironmentUnit(target, name, p, ordinal);

			Refresh(token);

			NotificationHubs.EnvironmentUnitChanged(token);
		}

		public void Delete(Guid token)
		{
			var target = GetByToken(token);

			if (target == null)
				throw new SysException(SR.ErrEnvironmentUnitNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.DeleteEnvironmentUnit(target);

			Refresh(token);

			NotificationHubs.EnvironmentUnitRemoved(token);
		}
	}
}
