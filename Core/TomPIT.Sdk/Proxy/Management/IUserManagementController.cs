using System;

namespace TomPIT.Proxy.Management
{
	public interface IUserManagementController
	{
		void ChangeAvatar(Guid user, Guid avatar);
		void ChangePassword(Guid user, string existingPassword, string password);
	}
}
