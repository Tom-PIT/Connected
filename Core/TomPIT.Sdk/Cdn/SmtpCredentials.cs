using System.Text;

namespace TomPIT.Cdn
{
	public abstract class SmtpCredentials : ISmtpCredentials
	{
		public Encoding Encoding { get; set; } = Encoding.UTF8;
	}
}
