using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;

namespace TomPIT.Security
{
	internal class UserService : ContextCacheRepository<IUser, Guid>, IUserService, IUserNotification
	{
		public UserService(ISysContext server) : base(server, "user")
		{

		}

		public event UserChangedHandler UserChanged;

		public List<IUser> Query()
		{
			var u = Server.CreateUrl("User", "Query");

			return Server.Connection.Get<List<User>>(u).ToList<IUser>();
		}

		public IUser Select(string qualifier)
		{
			IUser r = null;

			if (Guid.TryParse(qualifier, out Guid g))
				r = Get(g);
			else if (qualifier.Contains("@"))
				r = Get(f => string.Compare(f.Email, qualifier, true) == 0);
			else
				r = Get(f => string.Compare(f.LoginName, qualifier, true) == 0);

			if (r != null)
				return r;

			var u = Server.CreateUrl("User", "Select")
				.AddParameter("qualifier", qualifier);

			r = Server.Connection.Get<User>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var r = Get(f => f.AuthenticationToken == token);

			if (r != null)
				return r;

			var u = Server.CreateUrl("User", "SelectByAuthenticationToken")
				.AddParameter("token", token);

			r = Server.Connection.Get<User>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public void ChangePassword(Guid user, string existingPassword, string password)
		{
			var u = Server.CreateUrl("UserManagement", "ChangePassword");
			var e = new JObject
			{
				{"user",user },
				{"existingPassword",existingPassword },
				{"newPassword",password }
			};

			Server.Connection.Post(u, e);
		}

		public void Logout(int user)
		{
			throw new NotImplementedException();
		}

		public void NotifyChanged(object sender, UserEventArgs e)
		{
			Remove(e.User);

			UserChanged?.Invoke(Server, e);
		}
	}
}