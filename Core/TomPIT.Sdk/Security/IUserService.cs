﻿using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	public delegate void UserChangedHandler(ITenant tenant, UserEventArgs e);

	public interface IUserService
	{
		event UserChangedHandler UserChanged;

		List<IUser> Query();
		IUser Select(string qualifier);

		IUser SelectByAuthenticationToken(Guid token);

		void Logout(int user);
		void ChangePassword(Guid user, string existingPassword, string password);
		void ChangeAvatar(Guid user, byte[] contentBytes, string contentType, string fileName);
	}
}