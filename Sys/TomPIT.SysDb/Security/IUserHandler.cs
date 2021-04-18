using System;
using System.Collections.Generic;
using TomPIT.Globalization;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
	public interface IUserHandler
	{
		List<IUser> Query();

		IUser Select(int id);
		IUser Select(string loginName);
		IUser Select(Guid token);
		IUser SelectByUrl(string url);
		IUser SelectByEmail(string email);
		IUser SelectByAuthenticationToken(Guid token);
		IUser SelectBySecurityCode(string code);

		void Insert(Guid token, string loginName, string url, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, ILanguage language, string timezone, bool notificationEnabled, string mobile, string phone, Guid avatar, Guid authenticationToken,
			DateTime passwordChange, string securityCode);

		void Update(IUser user, string loginName, string url, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, ILanguage language, string timezone, bool notificationEnabled, string mobile, string phone, Guid avatar,
			DateTime passwordChange, string securityCode);

		void UpdateLoginInformation(IUser user, Guid authenticationToken, DateTime lastLogin);

		string SelectPassword(IUser user);
		void UpdatePassword(IUser user, string password);
		void Delete(IUser user);

		void UpdateState(IUser user, byte[] state);
		byte[] SelectState(IUser user);

		List<IMembership> QueryMembership();
		List<IMembership> QueryMembership(IUser user);
		void InsertMembership(IUser user, Guid role);
		void DeleteMembership(IUser user, Guid role);
		IMembership SelectMembership(IUser user, Guid role);
	}
}
