using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class Roles : SynchronizedRepository<IRole, Guid>
	{
		public Roles(IMemoryCache container) : base(container, "role")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Security.Roles.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Security.Roles.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IRole Select(Guid token)
		{
			return Get(token,
				(f) =>
				{
					return Shell.GetService<IDatabaseService>().Proxy.Security.Roles.Select(token);
				});
		}

		public Guid Insert(string name)
		{
			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Security.Roles.Insert(token, name);

			Select(token);
			NotificationHubs.RoleChanged(token);

			return token;
		}

		public void Update(Guid token, string name)
		{
			var role = Select(token);

			if (role == null)
				throw new SysException(SR.ErrRoleNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Roles.Update(role, name);

			Refresh(role.Token);
			NotificationHubs.RoleChanged(role.Token);
		}

		public List<IRole> Query()
		{
			return All();
		}

		public void Delete(Guid token)
		{
			var role = Select(token);

			if (role == null)
				throw new SysException(SR.ErrRoleNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Roles.Delete(role);

			Remove(token);
			NotificationHubs.RoleChanged(token);
		}
	}
}