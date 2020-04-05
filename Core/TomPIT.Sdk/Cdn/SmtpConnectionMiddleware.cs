using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class SmtpConnectionMiddleware : MiddlewareComponent, ISmtpConnectionMiddleware
	{
		public ISmtpCredentials GetCredentials(SmtpCredentialsEventArgs e)
		{
			return OnGetCredentials(e);
		}

		protected virtual ISmtpCredentials OnGetCredentials(SmtpCredentialsEventArgs e)
		{
			return null;
		}

		public ISmtpServerDescriptor ResolveSmtpServer(DomainEventArgs e)
		{
			return OnResolveSmtpServer(e);
		}

		protected virtual ISmtpServerDescriptor OnResolveSmtpServer(DomainEventArgs e)
		{
			return null;
		}
	}
}
