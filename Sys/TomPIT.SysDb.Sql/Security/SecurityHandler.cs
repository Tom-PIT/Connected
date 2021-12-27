using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class SecurityHandler : ISecurityHandler
	{
		private IUserHandler _users = null;
		private IPermissionHandler _permissions = null;
		private IRoleHandler _roles = null;
		private IAuthenticationTokenHandler _authTokens = null;
		private IAlienHandler _aliens = null;
		private IXmlKeyHandler _xmlKeys = null;

		public IAlienHandler Aliens
		{
			get
			{
				if (_aliens == null)
					_aliens = new AlienHandler();

				return _aliens;
			}
		}

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

		public IAuthenticationTokenHandler AuthenticationTokens
		{
			get
			{
				if (_authTokens == null)
					_authTokens = new AuthenticationTokenHandler();

				return _authTokens;
			}
		}

		public IXmlKeyHandler XmlKeys
		{
			get
			{
				if (_xmlKeys == null)
					_xmlKeys = new XmlKeyHandler();

				return _xmlKeys;
			}
		}
	}
}
