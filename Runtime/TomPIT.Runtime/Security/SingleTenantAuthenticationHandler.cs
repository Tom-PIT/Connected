using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TomPIT.Security
{
	internal class SingleTenantAuthenticationHandler : AuthenticationHandlerBase<SingleTenantAuthenticationOptions>
	{
		public static string ValidIssuer { get; set; }
		public static string ValidAudience { get; set; }
		public static string IssuerSigningKey { get; set; }

		public SingleTenantAuthenticationHandler(IOptionsMonitor<SingleTenantAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
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
			{
				principal = ValidateBearer();

				if (principal == null)
				{
					principal = ValidateBasic();

					if (principal == null)
						return AuthenticateResult.NoResult();
				}
			}

			return AuthenticateResult.Success(
					new AuthenticationTicket(
					principal,
					new AuthenticationProperties(),
					Scheme.Name));
		}

	}
}

