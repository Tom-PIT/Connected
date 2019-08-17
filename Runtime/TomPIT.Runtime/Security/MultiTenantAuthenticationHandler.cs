using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TomPIT.Security
{
	internal class MultiTenantAuthenticationHandler : AuthenticationHandlerBase<MultiTenantAuthenticationOptions>
	{
		public MultiTenantAuthenticationHandler(IOptionsMonitor<MultiTenantAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			return Task.FromResult(ResolveResult());
		}

		private AuthenticateResult ResolveResult()
		{
			var principal = ValidateJwt();

			if (principal == null)
				return AuthenticateResult.NoResult();

			return AuthenticateResult.Success(
					new AuthenticationTicket(
					principal,
					new AuthenticationProperties(),
					Scheme.Name));
		}
	}
}

