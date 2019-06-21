using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public abstract class Operation<TReturnValue> : OperationBase, IOperation<TReturnValue>
	{
		protected Operation(IDataModelContext context):base(context)
		{
			
		}

		public TReturnValue Invoke()
		{
			Validate();

			return OnInvoke();
		}

		protected virtual TReturnValue OnInvoke()
		{
			return default;
		}
	}

	public abstract class AsyncOperation : OperationBase, IOperation, IAsyncOperation
	{
		private IEventCallback _callback = null;
		protected AsyncOperation(IDataModelContext context) : base(context)
		{

		}

		public bool Cancel { get; set; }
		internal bool Async { get; set; } = true;

		public IEventCallback Callback
		{
			get
			{
				if (_callback == null)
				{
					var path = GetType().FindAttribute<AsyncPathAttribute>();

					if (path == null)
						throw new RuntimeException(SR.ErrAsyncPathExpected).WithMetrics(Context);

					var apiQualifier = new ApiQualifier(Context, path.Path);
					var api = Context.Connection().GetService<IComponentService>().SelectConfiguration(apiQualifier.MicroService.Token, "Api", apiQualifier.Api) as IApi;
					var op = api.Operations.FirstOrDefault(f => string.Compare(f.Name, apiQualifier.Operation, true) == 0);

					if (op == null)
						throw new RuntimeException($"SR.ErrServiceOperationNotFound ({path.Path})");

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
				if (!((EventCallback)Callback).Attached)
					Context.Services.Cdn.Event("$", this, Callback);

				OnBeginInvoke();
			}
			else
				OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}

	public abstract class Operation : OperationBase, IOperation
	{
		protected Operation(IDataModelContext context) : base(context)
		{

		}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
