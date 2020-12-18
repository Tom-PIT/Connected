using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class UserManagementController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var loginName = body.Optional("loginName", string.Empty);
			var email = body.Optional("email", string.Empty);
			var status = body.Optional("status", UserStatus.Inactive);
			var firstName = body.Optional("firstName", string.Empty);
			var lastName = body.Optional("lastName", string.Empty);
			var description = body.Optional("description", string.Empty);
			var pin = body.Optional("pin", string.Empty);
			var language = body.Optional("language", Guid.Empty);
			var timezone = body.Optional("timezone", string.Empty);
			var notificationEnabled = body.Optional("notificationEnabled", false);
			var mobile = body.Optional("mobile", string.Empty);
			var phone = body.Optional("phone", string.Empty);
			var passwordChange = body.Optional("passwordChange", DateTime.MinValue);
			var securityCode = body.Optional("securityCode", string.Empty);

			return DataModel.Users.Insert(loginName, email, status, firstName, lastName, description, pin, language, timezone,
				notificationEnabled, mobile, phone, passwordChange, securityCode);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var token = body.Required<Guid>("user");
			var loginName = body.Optional("loginName", string.Empty);
			var email = body.Optional("email", string.Empty);
			var status = body.Optional("status", UserStatus.Inactive);
			var firstName = body.Optional("firstName", string.Empty);
			var lastName = body.Optional("lastName", string.Empty);
			var description = body.Optional("description", string.Empty);
			var pin = body.Optional("pin", string.Empty);
			var language = body.Optional("language", Guid.Empty);
			var timezone = body.Optional("timezone", string.Empty);
			var notificationEnabled = body.Optional("notificationEnabled", false);
			var mobile = body.Optional("mobile", string.Empty);
			var phone = body.Optional("phone", string.Empty);
			var passwordChange = body.Optional("passwordChange", DateTime.MinValue);
			var securityCode = body.Optional("securityCode", string.Empty);

			DataModel.Users.Update(token, loginName, email, status, firstName, lastName, description, pin, language, timezone,
				notificationEnabled, mobile, phone, passwordChange, securityCode);
		}

		[HttpPost]
		public void ChangeAvatar()
		{
			var body = FromBody();

			var token = body.Required<Guid>("user");
			var avatar = body.Required<Guid>("avatar");

			DataModel.Users.ChangeAvatar(token, avatar);
		}

		[HttpPost]
		public void ResetPassword()
		{
			var body = FromBody();

			var token = body.Required<Guid>("user");

			DataModel.Users.ResetPassword(token.ToString(), null);
		}

		[HttpPost]
		public void ChangePassword()
		{
			var body = FromBody();

			var token = body.Required<Guid>("user");
			var existing = body.Optional("existingPassword", string.Empty);
			var newPw = body.Required<string>("newPassword");

			DataModel.Users.UpdatePassword(token.ToString(), existing, newPw);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var token = body.Required<Guid>("user");

			DataModel.Users.Delete(token);
		}

		[HttpPost]
		public void InsertMembership()
		{
			var body = FromBody();

			var user = body.Required<Guid>("user");
			var role = body.Required<Guid>("role");

			DataModel.Membership.Insert(user, role);
		}

		[HttpPost]
		public void RemoveMembership()
		{
			var body = FromBody();

			var user = body.Required<Guid>("user");
			var role = body.Required<Guid>("role");

			DataModel.Membership.Delete(user, role);
		}
	}
}
