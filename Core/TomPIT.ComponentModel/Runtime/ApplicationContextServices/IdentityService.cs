using TomPIT.Security;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class IdentityService : IIdentityService
	{
		private IUser _user = null;
		private string _jwToken = string.Empty;

		public IdentityService(IApplicationContext context)
		{
			Context = context;
		}

		private IApplicationContext Context { get; }

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
						var ctx = Context.GetServerContext();

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

				if (string.IsNullOrWhiteSpace(_jwToken) && Context.GetIdentity() is IdentityService identity)
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
	}
}
