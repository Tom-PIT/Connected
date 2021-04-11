using System;
using TomPIT.Middleware.Interop;

namespace TomPIT.Security
{
	[Obsolete("Please use ProxyAuthorizationModel instead.")]
	public abstract class AuthorizationModel : MiddlewareProxy, IAuthorizationModel
	{
		[Obsolete("Please use GetValue().")]
		public T GetValueFromTarget<T>(string propertyName)
		{
			return GetValue<T>(propertyName);
		}
	}

	public abstract class ProxyAuthorizationModel : MiddlewareProxy, IAuthorizationModel
	{
	}
}
