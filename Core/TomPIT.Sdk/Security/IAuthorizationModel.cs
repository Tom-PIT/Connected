using System;
using TomPIT.Middleware.Interop;

namespace TomPIT.Security
{
	public interface IAuthorizationModel : IMiddlewareProxy
	{
		[Obsolete("Please use GetValue instead.")]
		T GetValueFromTarget<T>(string propertyName);
	}
}
