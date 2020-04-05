using System.Collections.Generic;
using System.Linq;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareMembershipService : MiddlewareObject, IMiddlewareMembershipService
	{
		public MiddlewareMembershipService(IMiddlewareContext context) : base(context)
		{

		}
		public List<IUser> QueryUsers(params string[] roles)
		{
			var result = new List<IUser>();

			foreach (var role in roles)
			{
				var d = Context.Tenant.GetService<IRoleService>().Select(role);

				if (d == null)
					continue;

				var membership = Context.Tenant.GetService<IAuthorizationService>().QueryMembershipForRole(d.Token);

				foreach (var m in membership)
				{
					if (result.FirstOrDefault(f => f.Token == m.User) != null)
						continue;

					var user = Context.Services.Identity.GetUser(m.User);

					if (user == null)
						continue;

					result.Add(user);
				}
			}

			return result;
		}
	}
}
