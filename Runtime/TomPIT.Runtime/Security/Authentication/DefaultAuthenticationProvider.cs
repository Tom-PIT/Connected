using System;
using TomPIT.Connectivity;
using TomPIT.Environment;
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
			var r = Instance.SysProxy.Authentication.Authenticate(userName, password);

			if (r.Success)
			{
				var user = Tenant.GetService<IUserService>().Select(userName);

				if (user is null)
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
						Token = r.Token
					};
				}
			}

			return r;
		}

		public IClientAuthenticationResult AuthenticateByPin(string user, string pin)
		{
			return Instance.SysProxy.Authentication.AuthenticateByPin(user, pin);
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

			var features = Shell.GetService<IRuntimeService>().Features;

			if (features.HasFlag(InstanceFeatures.Application))
				claim = AuthenticationTokenClaim.Application;

			if (features.HasFlag(InstanceFeatures.Worker))
				claim |= AuthenticationTokenClaim.Worker;

			if (features.HasFlag(InstanceFeatures.Cdn))
				claim |= AuthenticationTokenClaim.Cdn;

			if (features.HasFlag(InstanceFeatures.IoT))
				claim |= AuthenticationTokenClaim.IoT;

			if (features.HasFlag(InstanceFeatures.BigData))
				claim |= AuthenticationTokenClaim.BigData;

			if (features.HasFlag(InstanceFeatures.Search))
				claim |= AuthenticationTokenClaim.Search;

			if (features.HasFlag(InstanceFeatures.Rest))
				claim |= AuthenticationTokenClaim.Rest;

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

		public string RequestToken(InstanceFeatures features)
		{
			var svc = Tenant.GetService<IAuthorizationService>() as AuthorizationService;
			var token = svc.AuthenticationTokens.Select(features);

			if (token == null)
				return null;

			return token.Key;
		}
	}
}