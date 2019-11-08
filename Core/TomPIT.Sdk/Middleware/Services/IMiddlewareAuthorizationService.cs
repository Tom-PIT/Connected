using System;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareAuthorizationService
	{
		bool Authorize(string claim, string primaryKey);
		bool Authorize(string claim, string primaryKey, Guid user);
	}
}
