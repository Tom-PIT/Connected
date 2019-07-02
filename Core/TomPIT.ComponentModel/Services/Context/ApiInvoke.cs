using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
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

		public object Execute(IApiExecutionScope sender, Guid microService, string api, string operation, JObject arguments, IApiTransaction transaction, bool explicitIdentifier, bool synchronous = false)
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
				var args = new OperationInvokeArguments(ctx, op, arguments, transaction);
				var operationType = FindOperationType(microService, op);

				if (operationType != null)
				{
					if (synchronous)
					{
						if (operationType.IsSubclassOf(typeof(AsyncOperation)))
						{
							var opInstance = operationType.CreateInstance<AsyncOperation>(new object[] { args });

							opInstance.Async = false;

							if (arguments != null)
								Types.Populate(JsonConvert.SerializeObject(arguments), opInstance);

							var method = opInstance.GetType().GetMethod("Invoke");

							method.Invoke(opInstance, null);

							return null;
						}
					}

					if (HasReturnValue(operationType))
					{
						var opInstance = operationType.CreateInstance<IOperationBase>(new object[] { args });

						if (arguments != null)
							Types.Populate(JsonConvert.SerializeObject(arguments), opInstance);

						var method = opInstance.GetType().GetMethod("Invoke");

						return method.Invoke(opInstance, null);
					}
					else
					{
						var opInstance = operationType.CreateInstance<IOperation>(new object[] { args });

						if (arguments != null)
							Types.Populate(JsonConvert.SerializeObject(arguments), opInstance);

						opInstance.Invoke();

						return null;
					}
				}
				else
				{
					var r = Context.Connection().GetService<ICompilerService>().Execute(microService, op.Invoke, this, args);

					if (r is JObject)
					{
						result = ((JObject)r).DeepClone() as JObject;

						return result;
					}

					return r;
				}
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

		private Type FindOperationType(Guid microService, IApiOperation operation)
		{
			var script = Context.Connection().GetService<ICompilerService>().GetScript(microService, operation);

			if (script != null && script.Assembly == null && script.Errors.Count(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error) > 0)
			{
				var sb = new StringBuilder();

				foreach (var error in script.Errors)
					sb.AppendLine(error.Message);

				throw new RuntimeException(operation.Name, sb.ToString());
			}

			var assembly = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
			var target = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => string.Compare(f.ShortName(), script.Assembly, true) == 0);

			return target?.GetTypes().FirstOrDefault(f => string.Compare(f.Name, operation.Name, true) == 0);
		}

		private bool IsOperation(Type type)
		{
			if (type == null)
				return false;

			if (type.ImplementsInterface<IOperation>())
				return true;

			return HasReturnValue(type);
		}

		private bool HasReturnValue(Type type)
		{
			if (type == null)
				return false;

			return type.GetInterfaces().Any(f =>
				f.IsGenericType &&
				f.GetGenericTypeDefinition() == typeof(IOperation<>));
		}
	}
}
