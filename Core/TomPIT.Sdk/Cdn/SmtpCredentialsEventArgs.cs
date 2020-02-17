namespace TomPIT.Cdn
{
	public class SmtpCredentialsEventArgs : DomainEventArgs
	{
		public SmtpCredentialsEventArgs(string domain) : base(domain)
		{
		}
	}
}
