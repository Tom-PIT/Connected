using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.Security
{
	internal class UserManagementService : IUserManagementService
	{
		public UserManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string password, string pin, Guid language, string timezone,
			bool notificationEnabled, string mobile, string phone)
		{
			var u = Connection.CreateUrl("UserManagement", "Insert");
			var e = new JObject
			{
				{"email", email},
				{"loginName", loginName},
				{"firstName", firstName},
				{"lastName", lastName},
				{"description", description},
				{"password", password},
				{"pin", pin},
				{"language", language},
				{"timezone", timezone},
				{"notificationEnabled", notificationEnabled},
				{"mobile", mobile},
				{"phone", phone}
			};

			return Connection.Post<Guid>(u, e);
		}

		public void ResetPassword(Guid user)
		{
			var u = Connection.CreateUrl("UserManagement", "ResetPassword");
			var e = new JObject
			{
				{"user", user}
			};

			Connection.Post(u, e);
		}

		public void Delete(Guid user)
		{
			var u = Connection.CreateUrl("UserManagement", "Delete");
			var e = new JObject
			{
				{"user", user}
			};

			Connection.Post<Guid>(u, e);

			if (Connection.GetService<IUserService>() is IUserNotification n)
				n.NotifyChanged(this, new UserEventArgs(user));
		}

		public void Update(Guid user, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone)
		{
			var u = Connection.CreateUrl("UserManagement", "Update");
			var e = new JObject
			{
				{"email", email},
				{"loginName", loginName},
				{"firstName", firstName},
				{"lastName", lastName},
				{"description", description},
				{"status", status.ToString()},
				{"user", user},
				{"pin", pin},
				{"language", language},
				{"timezone", timezone},
				{"notificationEnabled", notificationEnabled},
				{"mobile", mobile},
				{"phone", phone}
			};

			Connection.Post<Guid>(u, e);

			if (Connection.GetService<IUserService>() is IUserNotification n)
				n.NotifyChanged(this, new UserEventArgs(user));
		}
	}
}
