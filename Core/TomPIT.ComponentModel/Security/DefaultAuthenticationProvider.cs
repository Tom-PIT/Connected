using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Security
{
	internal class DefaultAuthenticationProvider : IAuthenticationProvider
	{
		public DefaultAuthenticationProvider(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public IClientAuthenticationResult Authenticate(string userName, string password)
		{
			var u = Server.CreateUrl("Authentication", "Authenticate");
			var args = new JObject
			{
				{ "user", userName },
				{"password", password }
			};

			var r = Server.Connection.Post<AuthenticationResult>(u, args);

			if (r.Success)
			{
				var user = Server.GetService<IUserService>().Select(userName);

				if (user != null)
					r.Identity = new Identity(user, r.Token, Server.Url);
			}

			return r;
		}

		public IClientAuthenticationResult Authenticate(string bearerKey)
		{
			/*
			 * TODO: implement bearer keys list
			 */
			return new AuthenticationResult
			{
				Success = false,
				Reason = AuthenticationResultReason.Other
			};
		}
	}
}
