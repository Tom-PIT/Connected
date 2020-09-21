using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security.Authentication
{
	public abstract class AuthenticationHandlerBase<T> : AuthenticationHandler<T> where T : AuthenticationSchemeOptions, new()
	{
		protected AuthenticationHandlerBase(IOptionsMonitor<T> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected ClaimsPrincipal ValidateJwt()
		{
			if (Request == null || !Request.Cookies.ContainsKey(SecurityUtils.AuthenticationCookieName))
				return null;

			var cookie = Request.Cookies[SecurityUtils.AuthenticationCookieName];
			var json = Serialization.Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));

			var expiration = json.Optional("expiration", 0L);

			if (expiration > 0)
			{
				var dt = new DateTime(expiration);

				if (dt < DateTime.UtcNow)
					return null;
			}

			var jwt = json.Required<string>("jwt");
			var endpoint = json.Required<string>("endpoint");

			var handler = new JwtSecurityTokenHandler();
			TokenValidationParameters pars = null;

			try
			{
				pars = Shell.GetService<IConnectivityService>().SelectTenant(endpoint).ValidationParameters;
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
			ITenant tenant = null;

			try
			{
				/*
				 * a lot of things can go wrong here. in the case of any error we just log off the user
				 */
				tenant = Shell.GetService<IConnectivityService>().SelectTenant(endpoint);
				user = tenant.GetService<IUserService>().SelectByAuthenticationToken(at);
			}
			catch { }

			if (user == null)
				return null;

			var id = new Identity(user, jwt, endpoint);
			var pr = new Principal(id);

			return pr;
		}

		protected ClaimsPrincipal ValidateBasic()
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

			var r = MiddlewareDescriptor.Current.Tenant.GetService<IAuthorizationService>().Authenticate(userName, password);

			if (!r.Success)
				return null;

			return new Principal(r.Identity);
		}

		protected ClaimsPrincipal ValidateBearer()
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

				try
				{
					key = Encoding.UTF8.GetString(Convert.FromBase64String(tokens[1]));
				}
				catch
				{
					return null;
				}
			}

			if (string.IsNullOrWhiteSpace(key))
				return null;

			var r = MiddlewareDescriptor.Current.Tenant.GetService<IAuthorizationService>().Authenticate(key);

			if (!r.Success)
				return null;

			return new Principal(r.Identity);
		}

		protected ClaimsPrincipal ValidateSSO()
		{
			var header = Request.Headers["Authorization"];
			var token = Guid.Empty;

			if (header.Count == 0)
				return null;

			var content = header[0];
			var tokens = content.Split(' ');

			if (tokens.Length != 2)
				return null;

			if (string.Compare(tokens[0], "sso", true) != 0)
				return null;

			try
			{
				token = Types.Convert<Guid>(Encoding.UTF8.GetString(Convert.FromBase64String(tokens[1])));
			}
			catch
			{
				return null;
			}

			if (token == Guid.Empty)
				return null;

			var r = MiddlewareDescriptor.Current.Tenant.GetService<IAuthorizationService>().Authenticate(token);

			if (!r.Success)
				return null;

			return new Principal(r.Identity);
		}
	}
}
