using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Security.Authentication
{
	internal class DefaultAuthenticationProvider : TenantObject, IAuthenticationProvider
	{
		public DefaultAuthenticationProvider(ITenant tenant) : base(tenant)
		{
		}

		public IClientAuthenticationResult Authenticate(string userName, string password)
		{
			var u = Tenant.CreateUrl("Authentication", "Authenticate");
			var args = new JObject
			{
				{ "user", userName },
				{"password", password }
			};

			var r = Tenant.Post<AuthenticationResult>(u, args);

			if (r.Success)
			{
				var user = Tenant.GetService<IUserService>().Select(userName);

				if (user != null)
					r.Identity = new Identity(user, r.Token, Tenant.Url);
			}

			return r;
		}

		public IClientAuthenticationResult Authenticate(Guid authenticationToken)
		{
			var user = Tenant.GetService<IUserService>().SelectByAuthenticationToken(authenticationToken);

			if (user == null)
			{
				return new AuthenticationResult
				{
					Success = false,
					Reason = AuthenticationResultReason.Other
				};
			}
			else
			{
				return new AuthenticationResult
				{
					Identity = new Identity(user),
					Reason = AuthenticationResultReason.OK,
					Success = true,
					Token = user.AuthenticationToken.ToString()
				};
			}
		}
		public IClientAuthenticationResult AuthenticateByPin(string user, string pin)
		{
			var u = Tenant.GetService<IUserService>().Select(user);

			if (user == null)
			{
				return new AuthenticationResult
				{
					Success = false,
					Reason = AuthenticationResultReason.Other
				};
			}
			else
			{
				if (string.IsNullOrWhiteSpace(pin) || string.Compare(u.Pin, pin, false) != 0)
				{
					return new AuthenticationResult
					{
						Success = false,
						Reason = AuthenticationResultReason.InvalidCredentials
					};
				}

				return new AuthenticationResult
				{
					Identity = new Identity(u),
					Reason = AuthenticationResultReason.OK,
					Success = true,
					Token = u.AuthenticationToken.ToString()
				};
			}
		}
		public IClientAuthenticationResult Authenticate(string authenticationToken)
		{
			var svc = Tenant.GetService<IAuthorizationService>() as AuthorizationService;

			var token = svc.AuthenticationTokens.Select(authenticationToken);

			if (token == null)
			{
				return new AuthenticationResult
				{
					Success = false,
					Reason = AuthenticationResultReason.InvalidToken
				};
			}

			var u = Tenant.GetService<IUserService>().Select(token.User.ToString());
			var claim = AuthenticationTokenClaim.None;

			switch (Shell.GetService<IRuntimeService>().Type)
			{
				case InstanceType.Application:
					claim = AuthenticationTokenClaim.Application;
					break;
				case InstanceType.Worker:
					claim = AuthenticationTokenClaim.Worker;
					break;
				case InstanceType.Cdn:
					claim = AuthenticationTokenClaim.Cdn;
					break;
				case InstanceType.IoT:
					claim = AuthenticationTokenClaim.IoT;
					break;
				case InstanceType.BigData:
					claim = AuthenticationTokenClaim.BigData;
					break;
				case InstanceType.Search:
					claim = AuthenticationTokenClaim.Search;
					break;
				case InstanceType.Rest:
					claim = AuthenticationTokenClaim.Rest;
					break;
			}

			if (token == null || !token.IsValid(Shell.HttpContext.Request, claim))
			{
				return new AuthenticationResult
				{
					Success = false,
					Reason = AuthenticationResultReason.Other
				};
			}
			else
			{
				return new AuthenticationResult
				{
					Identity = new Identity(u),
					Reason = AuthenticationResultReason.OK,
					Success = true,
					Token = u.AuthenticationToken.ToString()
				};
			}
		}

		public string RequestToken(InstanceType type)
		{
			var svc = Tenant.GetService<IAuthorizationService>() as AuthorizationService;
			var token = svc.AuthenticationTokens.Select(type);

			if (token == null)
				return null;

			return token.Key;
		}
	}
}