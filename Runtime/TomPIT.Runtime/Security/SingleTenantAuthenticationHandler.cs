using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TomPIT.Server.Security;

namespace TomPIT.Security
{
	internal class SingleTenantAuthenticationHandler : AuthenticationHandler<SingleTenantAuthenticationOptions>
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
			var principal = ValidateBearer();

			if (principal == null)
			{
				principal = ValidateBasic();

				if (principal == null)
					return AuthenticateResult.NoResult();
			}

			return AuthenticateResult.Success(
					new AuthenticationTicket(
					principal,
					new AuthenticationProperties(),
					Scheme.Name));
		}

		private ClaimsPrincipal ValidateBasic()
		{
			var header = Request.Headers["Authorization"];

			if (header.Count == 0)
				return null;

			var content = header[0];
			var tokens = content.Split(' ');

			if (tokens.Length != 2)
				return null;

			if (string.Compare(tokens[0], "basic", true) != 0)
				return null;

			var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(tokens[1]));
			var credentialTokens = credentials.Split(new char[] { ':' }, 2);

			var userName = credentialTokens[0];
			var password = credentialTokens[1];

			var r = Instance.Connection.GetService<IAuthorizationService>().Authenticate(userName, password);

			if (!r.Success)
				return null;

			return new Principal(r.Identity);
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

			return new Principal(r.Identity);
		}
	}
}

