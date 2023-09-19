using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class RoleService : SynchronizedClientRepository<IRole, Guid>, IRoleService, IRoleNotification
	{
		public RoleService(ITenant tenant) : base(tenant, "role")
		{

		}

		protected override void OnInitializing()
		{
			var ds = Instance.SysProxy.Roles.Query();

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

		public ImmutableList<IRole> Query()
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

			var role = Instance.SysProxy.Roles.Select(e.Role);

			if (role is not null)
				Set(role.Token, role, TimeSpan.Zero);
		}

		public IRole Select(string name)
		{
			return Get(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		public void Delete(Guid token)
		{
			Instance.SysProxy.Management.Roles.Delete(token);

			if (Tenant.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}

		public Guid Insert(string name)
		{
			var id = Instance.SysProxy.Management.Roles.Insert(name);

			if (Tenant.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(id));

			return id;
		}
		public void Update(Guid token, string name)
		{
			Instance.SysProxy.Management.Roles.Update(token, name);

			if (Tenant.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}
	}
}
