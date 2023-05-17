using System;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
	internal class UserManagementController : IUserManagementController
	{
		public void ChangeAvatar(Guid user, Guid avatar)
		{
			DataModel.Users.ChangeAvatar(user, avatar);
		}

		public void ChangePassword(Guid user, string existingPassword, string password)
		{
			DataModel.Users.UpdatePassword(user.ToString(), existingPassword, password);
		}
	}
}
