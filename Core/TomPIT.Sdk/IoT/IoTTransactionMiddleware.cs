using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.IoT
{
	public abstract class IoTTransactionMiddleware : MiddlewareComponent, IIoTTransactionMiddleware
	{
		public void Invoke()
		{
			Validate();
			AuthorizePolicies();
			OnAuthorize();
			OnInvoke();
		}

		private void AuthorizePolicies()
		{
			Context.Tenant.GetService<IAuthorizationService>().AuthorizePolicies(Context, this);
		}

		protected virtual void OnAuthorize()
		{

		}

		protected virtual void OnInvoke()
		{

		}

		public override string ToString()
		{
			return GetType().ShortName();
		}
	}
}
