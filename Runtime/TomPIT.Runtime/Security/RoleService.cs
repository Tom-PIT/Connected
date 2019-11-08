using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	internal class RoleService : SynchronizedClientRepository<IRole, Guid>, IRoleService, IRoleNotification
	{
		public RoleService(ITenant tenant) : base(tenant, "role")
		{

		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("Role", "Query");
			var ds = Tenant.Get<List<Role>>(u).ToList<IRole>();

			Set(SecurityUtils.FullControlRole, new SystemRole(SecurityUtils.FullControlRole, "Full control", RoleBehavior.Explicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.AuthenticatedRole, new SystemRole(SecurityUtils.AuthenticatedRole, "Authenticated", RoleBehavior.Implicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.AnonymousRole, new SystemRole(SecurityUtils.AnonymousRole, "Anonymous", RoleBehavior.Implicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.EveryoneRole, new SystemRole(SecurityUtils.EveryoneRole, "Everyone", RoleBehavior.Implicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.DomainIdentityRole, new SystemRole(SecurityUtils.DomainIdentityRole, "Domain identity", RoleBehavior.Implicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.LocalIdentityRole, new SystemRole(SecurityUtils.LocalIdentityRole, "Local identity", RoleBehavior.Implicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.Development, new SystemRole(SecurityUtils.Development, "Development", RoleBehavior.Explicit, RoleVisibility.Hidden), TimeSpan.Zero);
			Set(SecurityUtils.Management, new SystemRole(SecurityUtils.Management, "Management", RoleBehavior.Explicit, RoleVisibility.Hidden), TimeSpan.Zero);

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		public List<IRole> Query()
		{
			return All();
		}

		public IRole Select(Guid token)
		{
			return Get(token);
		}

		public void NotifyChanged(object sender, RoleEventArgs e)
		{
			Remove(e.Role);

			var u = Tenant.CreateUrl("Role", "Select")
				.AddParameter("token", e.Role);

			var role = Tenant.Get<Role>(u);

			if (role != null)
				Set(role.Token, role, TimeSpan.Zero);
		}

		public IRole Select(string name)
		{
			return Get(f => string.Compare(f.Name, name, true) == 0);
		}
	}
}
