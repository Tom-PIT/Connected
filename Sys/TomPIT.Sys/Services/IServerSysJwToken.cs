namespace TomPIT.Sys.Services
{
	public interface IServerSysJwToken
	{
		string ValidIssuer { get; }
		string ValidAudience { get; }
		string IssuerSigningKey { get; }
	}
}
