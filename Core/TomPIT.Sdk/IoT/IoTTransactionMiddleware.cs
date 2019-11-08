using TomPIT.Middleware;

namespace TomPIT.IoT
{
	public abstract class IoTTransactionMiddleware : MiddlewareComponent, IIoTTransactionMiddleware
	{
		protected IoTTransactionMiddleware(IMiddlewareContext context) : base(context)
		{
		}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
