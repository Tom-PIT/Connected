using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.Services.Context
{
	public class ApiInvoke : ContextService
	{
		public ApiInvoke(IExecutionContext context) : base(context)
		{
		}

		public object Execute(IApiExecutionScope sender, Guid microService, string api, string operation, JObject arguments, IApiTransaction transaction, bool explicitIdentifier)
		{
			var ms = ResolveMicroService(sender, microService);
			var ctx = new ExecutionContext(Context);

			ctx.Identity.SetContextId(ms.ToString());

			var svc = GetApi(microService, api, explicitIdentifier);

			if (svc.MicroService(ctx.Connection()) != ms)
				CheckReference(ctx, ms, svc.MicroService(ctx.Connection()));

			switch (svc.Scope)
			{
				// must be inside the same microservice
				case ElementScope.Internal:
					if (svc.MicroService(Context.Connection()) != Context.MicroService())
						throw new ExecutionException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
				case ElementScope.Private:
					// must be inside the same api
					if (sender == null || sender.Api.Component != svc.Component)
						throw new ExecutionException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
			}

			var op = svc.Operations.FirstOrDefault(f => string.Equals(f.Name, operation, StringComparison.OrdinalIgnoreCase));

			if (op == null)
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrServiceOperationNotFound, operation), CreateDescriptor(ExecutionEvents.DataRead, ExecutionContextState.Api, null, null, ms));

			switch (op.Scope)
			{
				case ElementScope.Internal:
					if (svc.MicroService(Context.Connection()) != Context.MicroService())
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
				case ElementScope.Private:
					if (sender == null || sender.Api.Component != svc.Component)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
			}

			var prepare = Prepare(ctx, op, arguments);

			if (prepare.Async)
				return null;

			var args = new OperationInvokeArguments(ctx, op, arguments, transaction);

			var r = Context.Connection().GetService<ICompilerService>().Execute(ms, op.Invoke, this, args);

			if (transaction != null)
				transaction.Notify(op);

			if (r is JObject)
				return ((JObject)r).DeepClone() as JObject;

			return r;
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

			Context.Connection().GetService<ICompilerService>().Execute(context.Identity.ContextId.AsGuid(), operation.Prepare, this, args);

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
					Authority = "Api",
					AuthorityId = operation.Closest<IApi>().Component.AsString(),
					ContextProperty = operation.Id.AsString(),
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					MicroService = operation.MicroService(Context.Connection()),
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
					Authority = "Api",
					AuthorityId = operation.Closest<IApi>().Component.AsString(),
					ContextProperty = operation.Id.AsString(),
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					MicroService = operation.MicroService(Context.Connection()),
					Source = "Rollback"
				};

				Context.Connection().GetService<Diagnostics.ILoggingService>().Write(le);
			}
		}
	}
}
