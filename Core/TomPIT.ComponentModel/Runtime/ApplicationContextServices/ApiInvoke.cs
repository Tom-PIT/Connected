using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Compilers;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public class ApiInvoke : Service
	{
		public ApiInvoke(IApplicationContext context) : base(context)
		{
		}

		public object Execute(IApiExecutionScope sender, Guid microService, string api, string operation, JObject arguments, IApiTransaction transaction, bool explicitIdentifier)
		{
			var ms = ResolveMicroService(sender, microService);
			var ctx = new ApplicationContext(Context);

			ctx.Identity.SetContextId(ms.ToString());

			var svc = GetApi(ms, api, explicitIdentifier);

			if (svc.MicroService(ctx.GetServerContext()) != ms)
				CheckReference(ctx, ms, svc.MicroService(ctx.GetServerContext()));

			switch (svc.Scope)
			{
				// must be inside the same microservice
				case ElementScope.Internal:
					if (svc.MicroService(Context.GetServerContext()) != Context.MicroService())
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
				case ElementScope.Private:
					// must be inside the same api
					if (sender == null || sender.Api.Component != svc.Component)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, api, operation));
					break;
			}

			var op = svc.Operations.FirstOrDefault(f => string.Equals(f.Name, operation, StringComparison.OrdinalIgnoreCase));

			if (op == null)
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrServiceOperationNotFound, operation), CreateDescriptor(RuntimeEvents.DataRead, ExecutionContext.Api, null, null, ms));

			switch (op.Scope)
			{
				case ElementScope.Internal:
					if (svc.MicroService(Context.GetServerContext()) != Context.MicroService())
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

			var r = Context.GetServerContext().GetService<ICompilerService>().Execute(ms, op.Invoke, this, args);

			if (transaction != null)
				transaction.Notify(op);

			if (r is JObject)
				return ((JObject)r).DeepClone() as JObject;

			return r;
		}

		private void CheckReference(IApplicationContext context, Guid microService, Guid requiredReference)
		{
			var ms = context.GetServerContext().GetService<IMicroServiceService>().Select(microService);
			var reference = context.GetServerContext().GetService<IMicroServiceService>().Select(microService);


			ms.ValidateMicroServiceReference(context.GetServerContext(), reference == null ? requiredReference.ToString() : reference.Name);
		}

		private void CheckReference(IApplicationContext context, string microService, string requiredReference)
		{
			var ms = context.GetServerContext().GetService<IMicroServiceService>().Select(microService);

			ms.ValidateMicroServiceReference(context.GetServerContext(), requiredReference);
		}

		private Guid ResolveMicroService(IApiExecutionScope sender, Guid microService)
		{
			if (sender != null)
				return sender.Api.MicroService(Context.GetServerContext());

			return microService;
		}

		private OperationPrepareArguments Prepare(IApplicationContext context, IApiOperation operation, JObject arguments)
		{
			var args = new OperationPrepareArguments(context, operation, arguments);

			Context.GetServerContext().GetService<ICompilerService>().Execute(context.Identity.ContextId.AsGuid(), operation.Prepare, this, args);

			return args;
		}

		public void Commit(IApiOperation operation, IApiTransaction transaction)
		{
			var args = new OperationTransactionArguments(Context, operation, transaction);

			try
			{
				Context.GetServerContext().GetService<ICompilerService>().Execute(operation.MicroService(Context.GetServerContext()), operation.Commit, this, args);
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
					MicroService = operation.MicroService(Context.GetServerContext()),
					Source = "Commit"
				};

				Context.GetServerContext().GetService<Diagnostics.ILoggingService>().Write(le);
			}
		}

		public void Rollback(IApiOperation operation, IApiTransaction transaction)
		{
			var args = new OperationTransactionArguments(Context, operation, transaction);

			try
			{
				Context.GetServerContext().GetService<ICompilerService>().Execute(operation.MicroService(Context.GetServerContext()), operation.Commit, this, args);
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
					MicroService = operation.MicroService(Context.GetServerContext()),
					Source = "Rollback"
				};

				Context.GetServerContext().GetService<Diagnostics.ILoggingService>().Write(le);
			}
		}
	}
}
