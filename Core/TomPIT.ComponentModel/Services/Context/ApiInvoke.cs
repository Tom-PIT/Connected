using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Diagnostics;

namespace TomPIT.Services.Context
{
	public class ApiInvoke : ContextClient
	{
		public ApiInvoke(IExecutionContext context) : base(context)
		{
		}

		public object Execute(IApiExecutionScope sender, Guid microService, string api, string operation, JObject arguments, IApiTransaction transaction, bool explicitIdentifier, bool skipPrepare = false)
		{
			var ms = ResolveMicroService(sender, microService);
			var ctx = new ExecutionContext(Context, Context.Connection().GetService<IMicroServiceService>().Select(microService));
			var svc = GetApi(microService, api, explicitIdentifier);

			if (svc.MicroService(ctx.Connection()) != ms)
				CheckReference(ctx, ms, svc.MicroService(ctx.Connection()));

			switch (svc.Scope)
			{
				// must be inside the same microservice
				case ElementScope.Internal:
					if (svc.MicroService(Context.Connection()) != Context.MicroService.Token)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation))
						{
							Component = svc.Component
						}.WithMetrics(ctx);

					break;
				case ElementScope.Private:
					// must be inside the same api
					if (sender == null || sender.Api.Component != svc.Component)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation))
						{
							Component = svc.Component
						}.WithMetrics(ctx);

					break;
			}

			var op = svc.Operations.FirstOrDefault(f => string.Equals(f.Name, operation, StringComparison.OrdinalIgnoreCase));

			if (op == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrServiceOperationNotFound, operation))
				{
					Component = svc.Component
				}.WithMetrics(ctx);
			}

			switch (op.Scope)
			{
				case ElementScope.Internal:
					if (svc.MicroService(Context.Connection()) != Context.MicroService.Token)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
				case ElementScope.Private:
					if (sender == null || sender.Api.Component != svc.Component)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
			}

			var metric = ctx.Services.Diagnostic.StartMetric(op.Metrics, op.Id, arguments);
			JObject result = null;

			try
			{
				if (!skipPrepare)
				{
					var prepare = Prepare(ctx, op, arguments);

					if (prepare.Async)
						return null;
				}

				var args = new OperationInvokeArguments(ctx, op, arguments, transaction);

				var r = Context.Connection().GetService<ICompilerService>().Execute(microService, op.Invoke, this, args);

				if (transaction != null)
					transaction.Notify(op);

				if (r is JObject)
				{
					result = ((JObject)r).DeepClone() as JObject;

					return result;
				}

				//result = new JObject
				//{
				//	{"result", r==null?"null":r.ToString() }
				//};

				return r;
			}
			catch (Exception ex)
			{
				ctx.Services.Diagnostic.StopMetric(metric, Diagnostics.SessionResult.Fail, new JObject
				{
					{"exception", ex.Message }
				});

				throw ex;
			}
			finally
			{
				ctx.Services.Diagnostic.StopMetric(metric, Diagnostics.SessionResult.Success, result);
			}
		}

		private void CheckReference(IExecutionContext context, Guid microService, Guid requiredReference)
		{
			var ms = context.Connection().GetService<IMicroServiceService>().Select(microService);
			var reference = context.Connection().GetService<IMicroServiceService>().Select(microService);

			ms.ValidateMicroServiceReference(context.Connection(), reference == null ? requiredReference.ToString() : reference.Name);
		}

		private void CheckReference(IExecutionContext context, string microService, string requiredReference)
		{
			var ms = context.Connection().GetService<IMicroServiceService>().Select(microService);

			ms.ValidateMicroServiceReference(context.Connection(), requiredReference);
		}

		private Guid ResolveMicroService(IApiExecutionScope sender, Guid microService)
		{
			if (sender != null)
				return sender.Api.MicroService(Context.Connection());

			return microService;
		}

		private OperationPrepareArguments Prepare(IExecutionContext context, IApiOperation operation, JObject arguments)
		{
			var args = new OperationPrepareArguments(context, operation, arguments);

			Context.Connection().GetService<ICompilerService>().Execute(operation.MicroService(Context.Connection()), operation.Prepare, this, args);

			return args;
		}

		public void Commit(IApiOperation operation, IApiTransaction transaction)
		{
			var args = new OperationTransactionArguments(Context, operation, transaction);

			try
			{
				Context.Connection().GetService<ICompilerService>().Execute(operation.MicroService(Context.Connection()), operation.Commit, this, args);
			}
			catch (Exception ex)
			{
				var le = new LogEntry
				{
					Category = "Api",
					Component = operation.Configuration().Component,
					Element = operation.Id,
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "Commit"
				};

				Context.Connection().GetService<Diagnostics.ILoggingService>().Write(le);
			}
		}

		public void Rollback(IApiOperation operation, IApiTransaction transaction)
		{
			var args = new OperationTransactionArguments(Context, operation, transaction);

			try
			{
				Context.Connection().GetService<ICompilerService>().Execute(operation.MicroService(Context.Connection()), operation.Commit, this, args);
			}
			catch (Exception ex)
			{
				var le = new LogEntry
				{
					Category = "Api",
					Component = operation.Configuration().Component,
					Element = operation.Id,
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = "Rollback"
				};

				Context.Connection().GetService<Diagnostics.ILoggingService>().Write(le);
			}
		}

		private IApi GetApi(Guid microService, string api, bool explicitIdentifier)
		{
			var component = Context.Connection().GetService<IComponentService>().SelectComponent(microService, "Api", api);

			if (component == null)
			{
				if (!explicitIdentifier)
					component = Context.Connection().GetService<IComponentService>().SelectComponent("Api", api);

				if (component == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrComponentNotFound, api)).WithMetrics(Context);
			}

			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IApi config))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrComponentCorrupted, api))
				{
					Component = component.Token
				}.WithMetrics(Context);
			}

			return config;
		}
	}
}
