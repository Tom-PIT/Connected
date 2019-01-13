namespace TomPIT.SysDb.Security
{
	public interface ISecurityHandler
	{
		IUserHandler Users { get; }
		IPermissionHandler Permissions { get; }
		IRoleHandler Roles { get; }
		IAuthenticationTokenHandler AuthenticationTokens { get; }
	}
}
