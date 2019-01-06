using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Globalization;
using TomPIT.Server.Security;

namespace TomPIT.Security
{
	internal class JwtAuthenticationHandler : AuthenticationHandler<JwtAuthenticationOptions>
	{
		public JwtAuthenticationHandler(IOptionsMonitor<JwtAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			return Task.FromResult(ResolveResult());
		}

		private AuthenticateResult ResolveResult()
		{
			var principal = ValidateJwt();

			SetCulture(principal);

			if (principal == null)
				return AuthenticateResult.NoResult();

			return AuthenticateResult.Success(
					new AuthenticationTicket(
					principal,
					new AuthenticationProperties(),
					Scheme.Name));
		}

		private void SetCulture(ClaimsPrincipal principal)
		{
			if (principal == null)
				return;

			if (!principal.Identity.IsAuthenticated)
				return;

			if (!(principal.Identity is Identity id))
				return;

			if (id.User.Language == Guid.Empty)
				return;

			var language = Shell.GetService<IConnectivityService>().Select(id.Endpoint).GetService<ILanguageService>().Select(id.User.Language);

			var ci = CultureInfo.GetCultureInfo(language.Lcid);

			if (ci == null)
				return;

			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
		}

		private ClaimsPrincipal ValidateJwt()
		{
			if (Request == null || !Request.Cookies.ContainsKey(SecurityUtils.AuthenticationCookieName))
				return null;

			var cookie = Request.Cookies[SecurityUtils.AuthenticationCookieName];
			var json = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

			var jwt = json.Required<string>("jwt");
			var endpoint = json.Required<string>("endpoint");

			var handler = new JwtSecurityTokenHandler();
			TokenValidationParameters pars = null;

			try
			{
				pars = Shell.GetService<IConnectivityService>().Select(endpoint).ValidationParameters;
			}
			catch { }

			if (pars == null)
				return null;

			var at = Guid.Empty;

			try
			{
				var principal = handler.ValidateToken(jwt, pars, out SecurityToken r);
				var claim = principal.FindFirst(f => string.Compare(f.Type, ClaimTypes.NameIdentifier, true) == 0);

				if (claim == null)
					return null;

				if (!Guid.TryParse(claim.Value, out at))
					return null;
			}
			catch
			{
				return null;
			}

			IUser user = null;
			ISysConnection sys = null;

			try
			{
				/*
				 * a lot of things can go wrong here. in the case of any error we just log off the user
				 */
				sys = Shell.GetService<IConnectivityService>().Select(endpoint);
				user = sys.GetService<IUserService>().SelectByAuthenticationToken(at);
			}
			catch { }

			if (user == null)
				return null;

			var id = new Identity(user, jwt, endpoint);
			var pr = new Principal(id);

			return pr;
		}
	}
}

