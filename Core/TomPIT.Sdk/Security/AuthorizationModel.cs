using TomPIT.Middleware;

namespace TomPIT.Security
{
	public abstract class AuthorizationModel : MiddlewareObject, IAuthorizationModel
	{
		public object AuthorizationTarget { get; set; }

		public T GetValueFromTarget<T>(string propertyName)
		{
			return SecurityExtensions.GetValueFromTarget<T>(this, propertyName);
		}
	}
}
