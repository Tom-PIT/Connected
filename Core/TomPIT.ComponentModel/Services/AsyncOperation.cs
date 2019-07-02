using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public abstract class AsyncOperation : OperationBase, IOperation, IAsyncOperation
	{
		private IEventCallback _callback = null;
		protected AsyncOperation(IDataModelContext context, string asyncPath) : base(context)
		{
			AsyncPath = asyncPath;
		}

		public bool Cancel { get; set; }
		private string AsyncPath { get; }
		internal bool Async { get; set; } = true;

		public IEventCallback Callback
		{
			get
			{
				if (_callback == null)
				{
					if (string.IsNullOrWhiteSpace(AsyncPath))
						throw new RuntimeException(SR.ErrAsyncPathExpected).WithMetrics(Context);

					var apiQualifier = new ApiQualifier(Context, AsyncPath);
					var api = Context.Connection().GetService<IComponentService>().SelectConfiguration(apiQualifier.MicroService.Token, "Api", apiQualifier.Api) as IApi;
					var op = api.Operations.FirstOrDefault(f => string.Compare(f.Name, apiQualifier.Operation, true) == 0);

					if (op == null)
						throw new RuntimeException($"SR.ErrServiceOperationNotFound ({AsyncPath})");

					_callback = new EventCallback(apiQualifier.MicroService.Token, api.Component, op.Id);
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

				if (!((EventCallback)Callback).Attached)
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
