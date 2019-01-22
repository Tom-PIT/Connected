using Newtonsoft.Json.Linq;
using System;
using TomPIT.Security;

namespace TomPIT.Services.Context
{
	internal class ContextIdentityService : IContextIdentityService
	{
		private IUser _user = null;
		private string _jwToken = string.Empty;

		public ContextIdentityService(IExecutionContext context)
		{
			Context = context;
		}

		private IExecutionContext Context { get; }

		public bool IsAuthenticated
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(ImpersonatedUser))
					return true;

				if (Context is IRequestContextProvider cp)
				{
					if (cp.Request == null)
						return false;

					var http = cp.Request.HttpContext;

					if (http.User == null || http.User.Identity == null)
						return false;

					return http.User.Identity.IsAuthenticated;
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

			return Shell.GetService<IUserService>().Select(qualifier == null ? string.Empty : qualifier.ToString());
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
				{"password", password}
			};

			Context.Connection().Post(u, e);

			return id;
		}
	}
}
