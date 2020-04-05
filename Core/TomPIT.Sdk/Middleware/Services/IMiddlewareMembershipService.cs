using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareMembershipService
	{
		List<IUser> QueryUsers(params string[] roles);
	}
}
