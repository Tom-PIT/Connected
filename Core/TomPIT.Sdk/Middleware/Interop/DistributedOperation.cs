using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
	public abstract class DistributedOperation : MiddlewareApiOperation, IOperation, IDistributedOperation
	{
		private IMiddlewareCallback _callback = null;
		private List<IOperationResponse> _responses = null;
		[Obsolete("Please use parameterless constructor")]
		protected DistributedOperation([CIP(CIP.ApiOperationProvider)] string callbackPath)
		{
			CallbackPath = callbackPath;
		}

		protected DistributedOperation()
		{
			var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);
			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			CallbackPath = $"{ms.Name}/{component.Name}/{GetType().ShortName()}";
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
				if (_callback is null)
				{
					if (string.IsNullOrWhiteSpace(CallbackPath))
						throw new RuntimeException(SR.ErrAsyncPathExpected);

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
			Invoke(null);
		}

		public void Invoke(IMiddlewareContext? context)
		{
			if (context is not null)
				this.WithContext(context);

			try
			{
				Validate();
				OnValidating();

				if (OperationTarget == DistributedOperationTarget.Distributed)
				{
					if (Context.Environment.IsInteractive)
					{
						AuthorizePolicies();
						OnAuthorizing();
						OnAuthorize();
					}

					OnBeginInvoke();
				}
				else
				{
					try
					{
						OnInvoke();
					}
					catch (Exception ex)
					{
						throw;
					}
					DependencyInjections.Invoke<object>(null);
				}

				Invoked();
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException)
			{
				Rollback();
				throw;
			}
			catch (Exception ex)
			{
				Rollback();
				var se = new ScriptException(this, TomPITException.Unwrap(this, ex));

				ExceptionDispatchInfo.Capture(se).Throw();
			}
		}

		protected internal override void OnCommitting()
		{
			base.OnCommitting();

			if (OperationTarget == DistributedOperationTarget.Distributed && !((MiddlewareCallback)Callback).Attached)
			{
				if (Context.Services.Identity.IsAuthenticated)
				{
					var args = JObject.FromObject(this);
					args["user$"] = Context.Services.Identity.User.Token;
				}

				Context.Services.Cdn.Events.TriggerEvent("$", this, Callback);
			}
		}

		protected virtual void OnInvoke()
		{
		}

		protected virtual void OnAuthorize()
		{
		}
	}
}
