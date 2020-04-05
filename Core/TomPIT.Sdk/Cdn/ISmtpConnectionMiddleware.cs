using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public interface ISmtpConnectionMiddleware : IMiddlewareComponent
	{
		ISmtpCredentials GetCredentials(SmtpCredentialsEventArgs e);
		ISmtpServerDescriptor ResolveSmtpServer(DomainEventArgs e);
	}
}
