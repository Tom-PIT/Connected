using System.Net;

namespace TomPIT.Cdn
{
	public interface ISmtpNetworkCredentials : ISmtpCredentials
	{
		ICredentials Credentials { get; }
	}
}
