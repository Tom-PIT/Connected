using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TomPIT.Server.Security;

namespace TomPIT.Security
{
	internal class BearerAuthenticationHandler : AuthenticationHandler<BearerAuthenticationOptions>
	{
		public static string ValidIssuer { get; set; }
		public static string ValidAudience { get; set; }
		public static string IssuerSigningKey { get; set; }

		public BearerAuthenticationHandler(IOptionsMonitor<BearerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			return Task.FromResult(ResolveResult());
		}

		private AuthenticateResult ResolveResult()
		{
			var principal = ValidateBearer();

			if (principal == null)
				return AuthenticateResult.NoResult();

			return AuthenticateResult.Success(
					new AuthenticationTicket(
					principal,
					new AuthenticationProperties(),
					Scheme.Name));
		}

		private ClaimsPrincipal ValidateBearer()
		{
			var header = Request.Headers["Authorization"];
			var key = string.Empty;

			/*
			 * Bearer spec allow access_token query string to be passed in cases where
			 * client apis does not allow passing request headers (e.g. WebSockets).
			 */
			if (header.Count == 0)
				key = Request.Query["access_token"];
			else
			{
				var content = header[0];
				var tokens = content.Split(' ');

				if (tokens.Length != 2)
					return null;

				if (string.Compare(tokens[0], "bearer", true) != 0)
					return null;

				key = tokens[1];
			}

			if (string.IsNullOrWhiteSpace(key))
				return null;

			var r = Instance.Connection.GetService<IAuthorizationService>().Authenticate(key);

			if (!r.Success)
				return null;

			var pr = new Principal(r.Identity);

			return pr;
		}
	}
}

