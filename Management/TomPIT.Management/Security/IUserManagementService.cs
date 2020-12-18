using System;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	public interface IUserManagementService
	{
		Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description,
			string password, string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, string securityCode);

		void Update(Guid user, string loginName, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, string securityCode);

		void ResetPassword(Guid user);
		void Delete(Guid user);
	}
}
