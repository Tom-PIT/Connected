using System;
using Newtonsoft.Json.Linq;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareIdentityService : MiddlewareObject, IMiddlewareIdentityService
	{
		private IUser _user = null;
		private string _jwToken = string.Empty;
		private string _impersonatedUser = null;

		public MiddlewareIdentityService(IMiddlewareContext context) : base(context)
		{
		}

		public bool IsAuthenticated
		{
			get
			{
				if (Context is MiddlewareContext mc && mc.Owner != null)
					return mc.Owner.Services.Identity.IsAuthenticated;

				if (!string.IsNullOrWhiteSpace(ImpersonatedUser))
					return true;


				if (Shell.HttpContext != null)
				{
					if (Shell.HttpContext.User == null || Shell.HttpContext.User.Identity == null)
						return false;

					return Shell.HttpContext.User.Identity.IsAuthenticated;
				}

				return false;
			}
		}

		internal string ImpersonatedUser
		{
			get
			{
				if (Context is MiddlewareContext mc && mc.Owner != null)
					return ((MiddlewareIdentityService)mc.Owner.Services.Identity).ImpersonatedUser;

				return _impersonatedUser;
			}
			set
			{
				if (Context is MiddlewareContext mc && mc.Owner != null)
					((MiddlewareIdentityService)mc.Owner.Services.Identity).ImpersonatedUser = value;
				else
					_impersonatedUser = value;
			}
		}

		public IUser User
		{
			get
			{
				if (!IsAuthenticated)
					return null;

				if (Context is MiddlewareContext mc && mc.Owner != null)
					return mc.Owner.Services.Identity.User;

				if (_user == null)
				{
					if (!string.IsNullOrWhiteSpace(ImpersonatedUser))
					{
						var ctx = Context.Tenant;

						if (ctx != null)
							return ctx.GetService<IUserService>().Select(ImpersonatedUser);
					}
					else
						_user = MiddlewareDescriptor.Current.User;
				}
				return _user;
			}
		}

		public IUser GetUser(object qualifier)
		{
			if (qualifier == null)
				return null;

			return Context.Tenant.GetService<IUserService>().Select(qualifier == null ? string.Empty : qualifier.ToString());
		}

		public IAuthenticationResult Authenticate(string user, string password)
		{
			return Context.Tenant.GetService<IAuthorizationService>().Authenticate(user, password);
		}

		public IAuthenticationResult Authenticate(string authenticationToken)
		{
			return Context.Tenant.GetService<IAuthorizationService>().Authenticate(authenticationToken);
		}

		public Guid InsertUser(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
			string timezone, bool notificationsEnabled, string mobile, string phone, string password, string securityCode = null)
		{
			var u = Context.Tenant.CreateUrl("UserManagement", "Insert");
			var e = new JObject
			{
				{"email", email},
				{"loginName", loginName},
				{"firstName", firstName},
				{"lastName", lastName},
				{"status", status.ToString()},
				{"description", description},
				{"password", password},
				{"pin", pin},
				{"language", language},
				{"timezone", timezone},
				{"notificationEnabled", notificationsEnabled},
				{"mobile", mobile},
				{"phone", phone},
				{"securityCode", securityCode }
			};

			var id = Context.Tenant.Post<Guid>(u, e);

			u = Context.Tenant.CreateUrl("UserManagement", "ChangePassword");
			e = new JObject
			{
				{"user", id},
				{"newPassword", password}
			};

			Context.Tenant.Post(u, e);

			return id;
		}

		public void UpdateUser(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
			string timezone, bool notificationsEnabled, string mobile, string phone, string securityCode = null)
		{
			var u = Context.Tenant.CreateUrl("UserManagement", "Update");
			var e = new JObject
			{
				{"user", token},
				{ "email", email},
				{"loginName", loginName},
				{"firstName", firstName},
				{"lastName", lastName},
				{"status", status.ToString()},
				{"description", description},
				{"pin", pin},
				{"language", language},
				{"timezone", timezone},
				{"notificationEnabled", notificationsEnabled},
				{"mobile", mobile},
				{"phone", phone},
				{"securityCode", securityCode }
			};

			Context.Tenant.Post(u, e);

			if (Context.Tenant.GetService<IUserService>() is IUserNotification n)
				n.NotifyChanged(this, new UserEventArgs(token));
		}

		public Guid InsertAlien(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
		{
			return Context.Tenant.GetService<IAlienService>().Insert(firstName, lastName, email, mobile, phone, language, timezone);
		}

		public IAlien GetAlien(string email)
		{
			return Context.Tenant.GetService<IAlienService>().Select(email);
		}

		public IAlien GetAlienByMobile(string mobile)
		{
			return Context.Tenant.GetService<IAlienService>().SelectByMobile(mobile);
		}

		public IAlien GetAlienByPhone(string phone)
		{
			return Context.Tenant.GetService<IAlienService>().SelectByPhone(phone);
		}
	}
}
