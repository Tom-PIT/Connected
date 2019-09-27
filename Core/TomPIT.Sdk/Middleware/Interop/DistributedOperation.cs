using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Diagostics;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class DistributedOperation : MiddlewareOperation, IOperation, IDistributedOperation
	{
		private IMiddlewareCallback _callback = null;
		protected DistributedOperation(string callbackPath)
		{
			CallbackPath = callbackPath;
		}

		public bool Cancel { get; set; }
		private string CallbackPath { get; }
		internal bool Distributed { get; set; } = true;

		public IMiddlewareCallback Callback
		{
			get
			{
				if (_callback == null)
				{
					if (string.IsNullOrWhiteSpace(CallbackPath))
						throw new RuntimeException(SR.ErrAsyncPathExpected).WithMetrics(Context);

					var descriptor = ComponentDescriptor.Api(Context, CallbackPath);

					descriptor.Validate();

					var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

					if (op == null)
						throw new RuntimeException($"SR.ErrServiceOperationNotFound ({CallbackPath})");

					_callback = new MiddlewareCallback(descriptor.MicroService.Token, descriptor.Component.Token, op.Id);
				}

				return _callback;
			}
		}

		protected virtual void OnBeginInvoke()
		{

		}

		public void Invoke()
		{
			Validate();

			if (Distributed)
			{
				OnBeginInvoke();

				if (!((MiddlewareCallback)Callback).Attached)
					Context.Services.Cdn.Event("$", this, Callback);
			}
			else
				OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
