using System;
using System.Collections.Generic;
using TomPIT.Net;

namespace TomPIT.Security
{
	public delegate void UserChangedHandler(ISysContext sender, UserEventArgs e);

	public interface IUserService
	{
		event UserChangedHandler UserChanged;

		List<IUser> Query();
		IUser Select(string qualifier);

		IUser SelectByAuthenticationToken(Guid token);

		void Logout(int user);
		void ChangePassword(Guid user, string existingPassword, string password);
	}
}