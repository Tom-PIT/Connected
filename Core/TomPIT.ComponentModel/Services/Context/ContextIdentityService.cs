using Newtonsoft.Json.Linq;
using System;
using TomPIT.Security;

namespace TomPIT.Services.Context
{
	internal class ContextIdentityService : ContextClient, IContextIdentityService
	{
		private IUser _user = null;
		private string _jwToken = string.Empty;

		public ContextIdentityService(IExecutionContext context) : base(context)
		{
		}

		public bool IsAuthenticated
		{
			get
			{
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

		internal string ImpersonatedUser { get; set; }

		public IUser User
		{
			get
			{
				if (!IsAuthenticated)
					return null;

				if (_user == null)
				{
					if (!string.IsNullOrWhiteSpace(ImpersonatedUser))
					{
						var ctx = Context.Connection();

						if (ctx != null)
							return ctx.GetService<IUserService>().Select(ImpersonatedUser);
					}
					else
						_user = Context.GetAuthenticatedUser();
				}
				return _user;
			}
		}

		public string JwToken
		{
			get
			{
				if (!IsAuthenticated || !string.IsNullOrWhiteSpace(ImpersonatedUser))
					return null;

				if (string.IsNullOrWhiteSpace(_jwToken) && Context.GetIdentity() is ContextIdentityService identity)
					_jwToken = identity.JwToken;

				return _jwToken;
			}
		}

		public IUser GetUser(object qualifier)
		{
			if (qualifier == null)
				return null;

			return Context.Connection().GetService<IUserService>().Select(qualifier == null ? string.Empty : qualifier.ToString());
		}

		public IAuthenticationResult Authenticate(string user, string password)
		{
			return Context.Connection().GetService<IAuthorizationService>().Authenticate(user, password);
		}

		public IAuthenticationResult Authenticate(string authenticationToken)
		{
			return Context.Connection().GetService<IAuthorizationService>().Authenticate(authenticationToken);
		}

		public Guid InsertUser(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
			string timezone, bool notificationsEnabled, string mobile, string phone, string password)
		{
			var u = Context.Connection().CreateUrl("UserManagement", "Insert");
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
				{"phone", phone}
			};

			var id = Context.Connection().Post<Guid>(u, e);

			u = Context.Connection().CreateUrl("UserManagement", "ChangePassword");
			e = new JObject
			{
				{"user", id},
				{"newPassword", password}
			};

			Context.Connection().Post(u, e);

			return id;
		}

		public void UpdateUser(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
			string timezone, bool notificationsEnabled, string mobile, string phone)
		{
			var u = Context.Connection().CreateUrl("UserManagement", "Update");
			var e = new JObject
			{
				{"token", token},
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
				{"phone", phone}
			};

			Context.Connection().Post(u, e);

			if (Context.Connection().GetService<IUserService>() is IUserNotification n)
				n.NotifyChanged(this, new UserEventArgs(token));
		}

		public Guid InsertAlien(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
		{
			return Context.Connection().GetService<IAlienService>().Insert(firstName, lastName, email, mobile, phone, language, timezone);
		}

		public IAlien GetAlien(string email)
		{
			return Context.Connection().GetService<IAlienService>().Select(email);
		}

		public IAlien GetAlienByMobile(string mobile)
		{
			return Context.Connection().GetService<IAlienService>().SelectByMobile(mobile);
		}

		public IAlien GetAlienByPhone(string phone)
		{
			return Context.Connection().GetService<IAlienService>().SelectByPhone(phone);
		}
	}
}
