using System;
using System.Linq;
using System.Reflection;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Security;
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

			using var ctx = new MicroServiceContext(descriptor.MicroService, Context.Tenant.Url).WithIdentity(Context);
			var contextMs = Context as IMicroServiceContext;

			switch (descriptor.Configuration.Scope)
			{
				case ElementScope.Internal:
				case ElementScope.Private:
					throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, descriptor.ComponentName, descriptor.Element))
					{
						Component = descriptor.Component.Token
					}.WithMetrics(ctx);
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
				case ElementScope.Private:
					throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrScopeError, descriptor.ComponentName, descriptor.Element));
			}

			var operationType = Context.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, op, op.Name);
			var elevation = ctx as IElevationContext;

			if (HasReturnValue(operationType))
			{
				using var opInstance = operationType.CreateInstance<IMiddlewareOperation>();

				opInstance.SetContext(ctx);

				if (elevation != null)
					elevation.AuthorizationOwner = opInstance;

				if (arguments != null)
					Serializer.Populate(arguments, opInstance, false);

				var method = GetInvoke(opInstance.GetType());

				try
				{
					return method.Invoke(opInstance, null);
				}
				catch (Exception ex)
				{
					throw TomPITException.Unwrap(opInstance, ex);
				}
			}
			else
			{
				using var opInstance = operationType.CreateInstance<IOperation>();

				if (opInstance is IDistributedOperation)
					ReflectionExtensions.SetPropertyValue(opInstance, nameof(IDistributedOperation.OperationTarget), synchronous ? DistributedOperationTarget.InProcess : DistributedOperationTarget.Distributed);

				opInstance.SetContext(ctx);

				if (elevation != null)
					elevation.AuthorizationOwner = opInstance;

				if (arguments != null)
					Serializer.Populate(arguments, opInstance);

				try
				{
					opInstance.Invoke();
				}
				catch (Exception ex)
				{
					throw TomPITException.Unwrap(opInstance, ex);
				}

				return null;
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
