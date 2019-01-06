using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Security;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	[AllowAnonymous]
	public class EnvironmentManagementController : SysController
	{
		[HttpPost]
		public IActionResult Setup()
		{
			var ev = DataModel.EnvironmentVariables.Select("Setup state");

			if (ev != null && Types.Convert<int>(ev.Value) != 0)
				return NotFound();

			var body = FromBody();

			var un = body.Required<string>("userName");
			var pw = body.Required<string>("password");
			var firstName = body.Optional("firstName", string.Empty);
			var lastName = body.Optional("lastName", string.Empty);
			var email = body.Optional("email", string.Empty);
			var description = body.Optional("description", string.Empty);
			var pin = body.Optional("pin", string.Empty);
			var mobile = body.Optional("mobile", string.Empty);
			var phone = body.Optional("phone", string.Empty);

			var id = DataModel.Users.Insert(un, email, UserStatus.Active, firstName, lastName,
				description, pin, Guid.Empty, null, true, mobile, phone, DateTime.MinValue);

			DataModel.Users.UpdatePassword(id.ToString(), null, pw);

			var u = DataModel.Users.Resolve(un);
			DataModel.Membership.Insert(u.Token, SecurityUtils.FullControlRole);

			return Ok();
		}
	}
}
