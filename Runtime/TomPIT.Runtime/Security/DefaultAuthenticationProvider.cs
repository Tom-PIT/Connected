using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Security
{
	internal class DefaultAuthenticationProvider : IAuthenticationProvider
	{
		public DefaultAuthenticationProvider(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public IClientAuthenticationResult Authenticate(string userName, string password)
		{
			var u = Connection.CreateUrl("Authentication", "Authenticate");
			var args = new JObject
			{
				{ "user", userName },
				{"password", password }
			};

			var r = Connection.Post<AuthenticationResult>(u, args);

			if (r.Success)
			{
				var user = Connection.GetService<IUserService>().Select(userName);

				if (user != null)
					r.Identity = new Identity(user, r.Token, Connection.Url);
			}

			return r;
		}

		public IClientAuthenticationResult Authenticate(string authenticationToken)
		{
			var svc = Connection.GetService<IAuthorizationService>() as AuthorizationService;

			var token = svc.AuthenticationTokens.Select(authenticationToken);
			var u = Connection.GetService<IUserService>().Select(token.User.ToString());
			var claim = AuthenticationTokenClaim.None;

			switch (Connection.GetService<IRuntimeService>().Type)
			{
				case Environment.InstanceType.Application:
					claim = AuthenticationTokenClaim.Application;
					break;
				case Environment.InstanceType.Worker:
					claim = AuthenticationTokenClaim.Worker;
					break;
				case Environment.InstanceType.Cdn:
					claim = AuthenticationTokenClaim.Cdn;
					break;
				case Environment.InstanceType.IoT:
					claim = AuthenticationTokenClaim.IoT;
					break;
				case Environment.InstanceType.BigData:
					claim = AuthenticationTokenClaim.BigData;
					break;
				case Environment.InstanceType.Search:
					claim = AuthenticationTokenClaim.Search;
					break;
				case Environment.InstanceType.Rest:
					claim = AuthenticationTokenClaim.Rest;
					break;
			}

			if (token == null || !token.IsValid(ExecutionContext.HttpRequest, u, claim))
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
	}
}