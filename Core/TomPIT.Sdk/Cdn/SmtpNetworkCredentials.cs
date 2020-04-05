using System.Net;

namespace TomPIT.Cdn
{
	public class SmtpNetworkCredentials : SmtpCredentials, ISmtpNetworkCredentials
	{
		public ICredentials Credentials { get; set; }
	}
}
