using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TomPIT.Security;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Security
{
	public class TomPITAuthenticationHandler : AuthenticationHandler<TomPITAuthenticationOptions>
	{
		public static string ValidIssuer { get; set; }
		public static string ValidAudience { get; set; }
		public static string IssuerSigningKey { get; set; }

		public TomPITAuthenticationHandler(IOptionsMonitor<TomPITAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
		{
			Response.StatusCode = 401;
			Response.Headers.Append(HeaderNames.WWWAuthenticate, "Bearer");

			await Task.CompletedTask;
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

			if (header.Count == 0)
				return null;

			var content = header[0];
			var tokens = content.Split(' ');

			if (tokens.Length != 2)
				return null;

			if (string.Compare(tokens[0], "bearer", true) != 0)
				return null;

			var key = tokens[1];

			var token = DataModel.AuthenticationTokens.Select(key);

			if (token == null)
				return null;

			IUser user = null;

			try
			{
				user = DataModel.Users.Select(token.User);
			}
			catch { }

			if (user == null)
				return null;

			if (!token.IsValid(Request, user, AuthenticationTokenClaim.System))
				return null;

			var id = new Identity(user);
			var pr = new Principal(id);

			return pr;
		}

		public static JwtSecurityToken CreateToken(Guid authenticationToken)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IssuerSigningKey));
			var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, authenticationToken.ToString())
			};

			return new JwtSecurityToken(issuer: ValidIssuer, audience: ValidAudience, claims: claims, expires: DateTime.Now.AddDays(30), signingCredentials: cred);
		}
	}
}

