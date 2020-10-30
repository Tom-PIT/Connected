using TomPIT.Middleware;

namespace TomPIT.Security
{
	public interface IAuthorizationModel : IMiddlewareObject
	{
		object AuthorizationTarget { get; set; }
		T GetValueFromTarget<T>(string propertyName);
	}
}
