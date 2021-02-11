using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	[AllowAnonymous]
	public class AuthenticationController : SysController
	{
		[HttpPost]
		public IAuthenticationResult Authenticate()
		{
			var body = FromBody();

			var user = body.Required<string>("user");
			var password = body.Required<string>("password");

			return DataModel.Users.Authenticate(user, password);
		}

		[HttpPost]
		public IAuthenticationResult AuthenticateByPin()
		{
			var body = FromBody();

			var user = body.Required<string>("user");
			var pin = body.Required<string>("pin");

			return DataModel.Users.AuthenticateByPin(user, pin);
		}
	}
}