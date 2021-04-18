using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	internal class UserManagementService : TenantObject, IUserManagementService
	{
		public UserManagementService(ITenant tenant) : base(tenant)
		{

		}

		public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string password, string pin, Guid language, string timezone,
			bool notificationEnabled, string mobile, string phone, string securityCode)
		{
			var u = Tenant.CreateUrl("UserManagement", "Insert");
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
				{"phone", phone},
				{"securityCode", securityCode }
			};

			return Tenant.Post<Guid>(u, e);
		}

		public void ResetPassword(Guid user)
		{
			var u = Tenant.CreateUrl("UserManagement", "ResetPassword");
			var e = new JObject
			{
				{"user", user}
			};

			Tenant.Post(u, e);
		}

		public void Delete(Guid user)
		{
			var u = Tenant.CreateUrl("UserManagement", "Delete");
			var e = new JObject
			{
				{"user", user}
			};

			Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<IUserService>() is IUserNotification n)
				n.NotifyChanged(this, new UserEventArgs(user));
		}

		public void Update(Guid user, string loginName, string email, UserStatus status, string firstName, string lastName,
			string description, string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, string securityCode)
		{
			var u = Tenant.CreateUrl("UserManagement", "Update");
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
				{"phone", phone},
				{"securityCode", securityCode }
			};

			Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<IUserService>() is IUserNotification n)
				n.NotifyChanged(this, new UserEventArgs(user));
		}
	}
}
