using System;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management
{
	internal class UserManagementController : IUserManagementController
	{
		private const string Controller = "UserManagement";

		public void ChangeAvatar(Guid user, Guid avatar)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ChangeAvatar"), new
			{
				user,
				avatar
			});
		}

		public void ChangePassword(Guid user, string existingPassword, string password)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ChangePassword"), new
			{
				user,
				newPassword = password,
				existingPassword
			});

		}
	}
}
