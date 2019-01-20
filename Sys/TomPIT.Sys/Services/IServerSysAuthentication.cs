namespace TomPIT.Sys.Services
{
	public interface IServerSysAuthentication
	{
		IServerSysJwToken JwToken { get; }
	}
}
