using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TomPIT.ComponentModel;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
	public abstract class DistributedOperation : MiddlewareOperation, IOperation, IDistributedOperation
	{
		private IMiddlewareCallback _callback = null;
		private List<IOperationResponse> _responses = null;
		protected DistributedOperation([CIP(CIP.ApiOperationProvider)]string callbackPath)
		{
			CallbackPath = callbackPath;
		}

		[JsonIgnore]
		public List<IOperationResponse> Responses
		{
			get
			{
				if (_responses == null)
					_responses = new List<IOperationResponse>();

				return _responses;
			}
		}

		protected bool ResponseSuccessfull
		{
			get
			{
				if (Responses.Count == 0)
					return true;

				foreach (var response in Responses)
				{
					switch (response.Result)
					{
						case ResponseResult.Objection:
							return false;
					}
				}

				return true;
			}
		}
		private string CallbackPath { get; }
		[JsonIgnore]
		public DistributedOperationTarget OperationTarget { get; private set; } = DistributedOperationTarget.Distributed;
		[JsonIgnore]
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
						throw new RuntimeException($"{SR.ErrServiceOperationNotFound} ({CallbackPath})");

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
			OnValidateDependencies();

			if (OperationTarget == DistributedOperationTarget.Distributed)
			{
				OnBeginInvoke();

				if (!((MiddlewareCallback)Callback).Attached)
					Context.Services.Cdn.Events.TriggerEvent("$", this, Callback);
			}
			else
			{
				OnInvoke();
				DependencyInjections.Invoke<object>(null);
			}

			Invoked();
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
