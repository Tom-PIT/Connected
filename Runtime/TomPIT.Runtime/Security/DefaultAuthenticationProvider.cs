using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;

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
