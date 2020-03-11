using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Interop
{
	public class ApiInvoker : MiddlewareObject
	{
		public ApiInvoker(IMiddlewareContext context) : base(context)
		{
		}

		public object Invoke<T>(IApiExecutionScope sender, ConfigurationDescriptor<IApiConfiguration> descriptor, T arguments, bool synchronous = false)
		{
			descriptor.Validate();

			ValidateReference(sender, descriptor);

			var ctx = new MicroServiceContext(descriptor.MicroService, Context.Tenant.Url);
			var contextMs = Context as IMicroServiceContext;

			switch (descriptor.Configuration.Scope)
			{
				// must be inside the same microservice
				case ElementScope.Internal:
					if (contextMs != null && descriptor.MicroService.Token != contextMs.MicroService.Token)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, descriptor.ComponentName, descriptor.Element))
						{
							Component = descriptor.Component.Token
						}.WithMetrics(ctx);

					break;
				case ElementScope.Private:
					// must be inside the same api
					if (sender == null || sender.Api.Component != descriptor.Component.Token)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, descriptor.ComponentName, descriptor.Element))
						{
							Component = descriptor.Component.Token
						}.WithMetrics(ctx);

					break;
			}

			var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Equals(f.Name, descriptor.Element, StringComparison.OrdinalIgnoreCase));

			if (op == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrServiceOperationNotFound, descriptor.Element))
				{
					Component = descriptor.Component.Token
				}.WithMetrics(ctx);
			}

			switch (op.Scope)
			{
				case ElementScope.Internal:
					if (contextMs != null && descriptor.MicroService.Token != contextMs.MicroService.Token)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, descriptor.ComponentName, descriptor.Element));
					break;
				case ElementScope.Private:
					if (sender == null || sender.Api.Component != descriptor.Component.Token)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, descriptor.ComponentName, descriptor.Element));
					break;
			}

			var metric = ctx.Services.Diagnostic.StartMetric(op.Metrics, op.Id, arguments);
			var success = true;
			JObject result = null;
			IMiddlewareComponent opInstance = null;

			try
			{
				var operationType = Context.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, op, op.Name);

				if (HasReturnValue(operationType))
				{
					opInstance = operationType.CreateInstance<IMiddlewareOperation>();

					opInstance.SetContext(ctx);

					if (arguments != null)
						Serializer.Populate(arguments, opInstance);

					var method = GetInvoke(opInstance.GetType());

					return method.Invoke(opInstance, null);
				}
				else
				{
					opInstance = operationType.CreateInstance<IOperation>();

					if (operationType is IDistributedOperation)
						ReflectionExtensions.SetPropertyValue(opInstance, nameof(IDistributedOperation.OperationTarget), synchronous ? DistributedOperationTarget.InProcess : DistributedOperationTarget.Distributed);

					opInstance.SetContext(ctx);

					if (arguments != null)
						Serializer.Populate(arguments, opInstance);

					((IOperation)opInstance).Invoke();

					return null;
				}
			}
			catch (Exception ex)
			{
				var resolvedException = TomPITException.Unwrap(opInstance, ex);
				success = false;

				ctx.Services.Diagnostic.StopMetric(metric, Diagnostics.SessionResult.Fail, new JObject
				{
					{"exception", $"{resolvedException.Source}/{resolvedException.Message}" }
				});

				throw resolvedException;
			}
			finally
			{
				if (success)
					ctx.Services.Diagnostic.StopMetric(metric, Diagnostics.SessionResult.Success, result);
			}
		}

		private MethodInfo GetInvoke(Type type)
		{
			var methods = type.GetMethods().Where(f => string.Compare(f.Name, "Invoke", false) == 0);

			foreach (var method in methods)
			{
				if (!method.ContainsGenericParameters)
					return method;
			}

			return null;
		}

		private bool HasReturnValue(Type type)
		{
			if (type == null)
				return false;

			return type.GetInterfaces().Any(f =>
				f.IsGenericType &&
				f.GetGenericTypeDefinition() == typeof(IOperation<>));
		}

		private void ValidateReference(IApiExecutionScope sender, ConfigurationDescriptor<IApiConfiguration> descriptor)
		{
			if (sender == null)
				return;

			var originMicroService = Context.Tenant.GetService<IMicroServiceService>().Select(sender.Api.MicroService());

			originMicroService.ValidateMicroServiceReference(descriptor.MicroServiceName);
		}
	}
}
