using System;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareIdentityService
	{
		bool IsAuthenticated { get; }
		IUser User { get; }

		IUser GetUser(object qualifier);

		IAuthenticationResult Authenticate(string user, string password);
		IAuthenticationResult Authenticate(string authenticationToken);

		Guid InsertUser(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled,
			string mobile, string phone, string password = null, string securityCode = null);

		void UpdateUser(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled,
			string mobile, string phone, string securityCode = null);

		Guid InsertAlien(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone);

		IAlien GetAlien(string email);
		IAlien GetAlienByMobile(string mobile);
		IAlien GetAlienByPhone(string phone);
	}
}
