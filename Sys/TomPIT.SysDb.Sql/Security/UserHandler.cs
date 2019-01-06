using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Globalization;
using TomPIT.Security;
using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class UserHandler : IUserHandler
	{
		public void Delete(IUser user)
		{
			var w = new Writer("tompit.user_del");

			w.CreateParameter("@id", user.GetId());

			w.Execute();
		}

		public void Insert(Guid token, string loginName, string url, string email, UserStatus status, string firstName, string lastName, string description,
			string pin, ILanguage language, string timezone, bool notificationEnabled, string mobile, string phone, Guid avatar, Guid authenticationToken,
			DateTime passwordChange)
		{
			var w = new Writer("tompit.user_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@login_name", loginName, true);
			w.CreateParameter("@url", url, true);
			w.CreateParameter("@email", email, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@first_name", firstName, true);
			w.CreateParameter("@last_name", lastName, true);
			w.CreateParameter("@description", description, true);
			w.CreateParameter("@password_change", passwordChange, true);
			w.CreateParameter("@pin", pin, true);
			w.CreateParameter("@language", language == null ? 0 : language.GetId(), true);
			w.CreateParameter("@timezone", timezone, true);
			w.CreateParameter("@notification_enabled", notificationEnabled);
			w.CreateParameter("@mobile", mobile, true);
			w.CreateParameter("@phone", phone, true);
			w.CreateParameter("@avatar", avatar, true);
			w.CreateParameter("@auth_token", authenticationToken);

			w.Execute();
		}

		public List<IUser> Query()
		{
			return new Reader<User>("tompit.user_que").Execute().ToList<IUser>();
		}

		public IUser Select(int id)
		{
			var r = new Reader<User>("tompit.user_sel");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}

		public IUser Select(string loginName)
		{
			var r = new Reader<User>("tompit.user_sel");

			r.CreateParameter("@login_name", loginName);

			return r.ExecuteSingleRow();
		}

		public IUser Select(Guid token)
		{
			var r = new Reader<User>("tompit.user_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();

		}

		public IUser SelectByEmail(string email)
		{
			var r = new Reader<User>("tompit.user_sel");

			r.CreateParameter("@email", email);

			return r.ExecuteSingleRow();

		}

		public IUser SelectByUrl(string url)
		{
			var r = new Reader<User>("tompit.user_sel");

			r.CreateParameter("@url", url);

			return r.ExecuteSingleRow();

		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var r = new Reader<User>("tompit.user_sel");

			r.CreateParameter("@auth_token", token);

			return r.ExecuteSingleRow();
		}

		public byte[] SelectState(IUser user)
		{
			var r = new Reader<UserState>("tompit.user_state_sel");

			r.CreateParameter("@user_id", user.GetId());

			return r.ExecuteSingleRow()?.Content;
		}

		public void Update(IUser user, string loginName, string url, string email, UserStatus status, string firstName, string lastName,
			string description, string pin, ILanguage language, string timezone, bool notificationEnabled, string mobile, string phone, Guid avatar,
			DateTime passwordChange)
		{
			var w = new Writer("tompit.user_upd");

			w.CreateParameter("@id", user.GetId());
			w.CreateParameter("@login_name", loginName, true);
			w.CreateParameter("@url", url, true);
			w.CreateParameter("@email", email, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@first_name", firstName, true);
			w.CreateParameter("@last_name", lastName, true);
			w.CreateParameter("@description", description, true);
			w.CreateParameter("@password_change", passwordChange, true);
			w.CreateParameter("@pin", pin, true);
			w.CreateParameter("@language", language == null ? 0 : language.GetId(), true);
			w.CreateParameter("@timezone", timezone, true);
			w.CreateParameter("@notification_enabled", notificationEnabled);
			w.CreateParameter("@mobile", mobile, true);
			w.CreateParameter("@phone", phone, true);
			w.CreateParameter("@avatar", avatar, true);

			w.Execute();
		}

		public void UpdateState(IUser user, byte[] state)
		{
			var w = new Writer("tompit.user_state_mdf");

			w.CreateParameter("@user_id", user.GetId());
			w.CreateParameter("@state", state);

			w.Execute();
		}

		public void UpdateLoginInformation(IUser user, Guid authenticationToken, DateTime lastLogin)
		{
			var w = new Writer("tompit.user_upd_login_info");

			w.CreateParameter("@auth_token", authenticationToken);
			w.CreateParameter("@id", user.GetId());
			w.CreateParameter("@last_login", lastLogin);

			w.Execute();
		}

		public List<IMembership> QueryMembership()
		{
			return new Reader<Membership>("tompit.membership_que").Execute().ToList<IMembership>();
		}

		public List<IMembership> QueryMembership(IUser user)
		{
			var r = new Reader<Membership>("tompit.membership_que");

			r.CreateParameter("@user", user.GetId());

			return r.Execute().ToList<IMembership>();
		}

		public IMembership SelectMembership(IUser user, Guid role)
		{
			var r = new Reader<Membership>("tompit.membership_sel");

			r.CreateParameter("@user", user.GetId());
			r.CreateParameter("@role", role);

			return r.ExecuteSingleRow();
		}

		public void InsertMembership(IUser user, Guid role)
		{
			var w = new Writer("tompit.membership_ins");

			w.CreateParameter("@user", user.GetId());
			w.CreateParameter("@role", role);

			w.Execute();
		}

		public void DeleteMembership(IUser user, Guid role)
		{
			var w = new Writer("tompit.membership_del");

			w.CreateParameter("@user", user.GetId());
			w.CreateParameter("@role", role);

			w.Execute();
		}

		public string SelectPassword(IUser user)
		{
			var r = new ScalarReader<string>("tompit.user_sel_password");

			r.CreateParameter("@id", user.GetId());

			return r.ExecuteScalar(string.Empty);
		}

		public void UpdatePassword(IUser user, string password)
		{
			var w = new Writer("tompit.user_upd_password");

			w.CreateParameter("@id", user.GetId());
			w.CreateParameter("@password", password, true);

			w.Execute();
		}
	}
}