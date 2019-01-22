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
	}
}
