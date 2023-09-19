using TomPIT.Middleware;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class AuthenticationController : IAuthenticationController
	{
		public IClientAuthenticationResult Authenticate(string user, string password)
		{
			var r = DataModel.Users.Authenticate(user, password);
			var result = new AuthenticationResult
			{
				Reason = r.Reason,
				Success = r.Success,
				Token = r.Token
			};

			if (r.Success)
			{
				var usr = Tenant.GetService<IUserService>().Select(user);

				if (usr is not null)
					result.Identity = new Identity(usr, r.Token, MiddlewareDescriptor.Current.Tenant.Url);
			}

			return result;
		}

		public IClientAuthenticationResult AuthenticateByPin(string user, string pin)
		{
			var r = DataModel.Users.AuthenticateByPin(user, pin);
			var result = new AuthenticationResult
			{
				Reason = r.Reason,
				Success = r.Success,
				Token = r.Token
			};

			if (r.Success)
			{
				var usr = Tenant.GetService<IUserService>().Select(user);

				if (usr is not null)
					result.Identity = new Identity(usr, r.Token, MiddlewareDescriptor.Current.Tenant.Url);
			}

			return result;
		}
	}
}
