using TomPIT.Middleware;

namespace TomPIT.Security
{
	public interface IAuthorizationModel : IMiddlewareObject
	{
		object Instance { get; set; }
		T GetValueFromInstance<T>(string propertyName);
	}
}
