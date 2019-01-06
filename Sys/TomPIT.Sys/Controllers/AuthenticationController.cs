using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Data;

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
	}
}