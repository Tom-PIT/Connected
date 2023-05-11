using Newtonsoft.Json.Linq;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class AuthenticationController : IAuthenticationController
	{
		private const string Controller = "Authentication";

		public IClientAuthenticationResult Authenticate(string user, string password)
		{
			var u = Connection.CreateUrl(Controller, "Authenticate");
			var args = new JObject
			{
				{ "user", user },
				{"password", password }
			};

			var r = Connection.Post<AuthenticationResult>(u, args);

			if (r.Success)
			{
				var usr = Tenant.GetService<IUserService>().Select(user);

				if (usr is not null)
					r.Identity = new Identity(usr, r.Token, MiddlewareDescriptor.Current.Tenant.Url);
			}

			return r;
		}

		public IClientAuthenticationResult AuthenticateByPin(string user, string pin)
		{
			var r = Connection.Post<AuthenticationResult>(Connection.CreateUrl(Controller, "AuthenticateByPin"), new
			{
				user,
				pin
			});

			if (r.Success)
			{
				var u = Tenant.GetService<IUserService>().Select(user);

				if (user != null)
					r.Identity = new Identity(u, r.Token, MiddlewareDescriptor.Current.Tenant.Url);
			}

			return r;
		}
	}
}
