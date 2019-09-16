using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Diagostics;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class AsyncOperation : MiddlewareOperation, IOperation, IAsyncOperation
	{
		private IMiddlewareCallback _callback = null;
		protected AsyncOperation(string asyncPath)
		{
			AsyncPath = asyncPath;
		}

		public bool Cancel { get; set; }
		private string AsyncPath { get; }
		internal bool Async { get; set; } = true;

		public IMiddlewareCallback Callback
		{
			get
			{
				if (_callback == null)
				{
					if (string.IsNullOrWhiteSpace(AsyncPath))
						throw new RuntimeException(SR.ErrAsyncPathExpected).WithMetrics(Context);

					var descriptor = ComponentDescriptor.Api(Context, AsyncPath);

					descriptor.Validate();

					var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

					if (op == null)
						throw new RuntimeException($"SR.ErrServiceOperationNotFound ({AsyncPath})");

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

			if (Async)
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

		public void SetAsyncState(bool async)
		{
			Async = async;
		}
	}
}
