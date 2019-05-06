using System;
using TomPIT.Security;

namespace TomPIT.Services.Context
{
	public interface IContextIdentityService
	{
		bool IsAuthenticated { get; }
		IUser User { get; }

		IUser GetUser(object qualifier);

		IAuthenticationResult Authenticate(string user, string password);
		IAuthenticationResult Authenticate(string authenticationToken);

		Guid InsertUser(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled,
			string mobile, string phone, string password);

		void UpdateUser(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled,
			string mobile, string phone);

		Guid InsertAlien(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone);

		IAlien GetAlien(string email);
		IAlien GetAlienByMobile(string mobile);
		IAlien GetAlienByPhone(string phone);
	}
}
