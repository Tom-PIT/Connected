using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class SecurityHandler : ISecurityHandler
	{
		private IUserHandler _users = null;
		private IPermissionHandler _permissions = null;
		private IRoleHandler _roles = null;

		public IUserHandler Users
		{
			get
			{
				if (_users == null)
					_users = new UserHandler();

				return _users;
			}
		}

		public IPermissionHandler Permissions
		{
			get
			{
				if (_permissions == null)
					_permissions = new PermissionHandler();

				return _permissions;
			}
		}

		public IRoleHandler Roles
		{
			get
			{
				if (_roles == null)
					_roles = new RoleHandler();

				return _roles;
			}
		}
	}
}
