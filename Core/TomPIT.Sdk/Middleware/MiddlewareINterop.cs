using System;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Exceptions;
using TomPIT.Middleware.Interop;
using TomPIT.Serialization;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	internal class MiddlewareInterop : MiddlewareObject, IMiddlewareInterop
	{
		public MiddlewareInterop(IMiddlewareContext context) : base(context)
		{

		}
		public R Invoke<R>([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)] string api)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);
			var result = invoker.Invoke(this as IApiExecutionScope, descriptor, EventArgs.Empty);

			return Marshall.Convert<R>(result);
		}

		public R Invoke<R, A>([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)] string api, [CIP(CIP.ApiOperationParameterProvider)] A e)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);
			var result = invoker.Invoke(this as IApiExecutionScope, descriptor, e);

			return Marshall.Convert<R>(result);
		}

		public dynamic Invoke<A>([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)] string api, [CIP(CIP.ApiOperationParameterProvider)] A e)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);

			return invoker.Invoke(this as IApiExecutionScope, descriptor, e);
		}

		public dynamic Invoke([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)] string api)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);

			return invoker.Invoke(this as IApiExecutionScope, descriptor, EventArgs.Empty);
		}

		public R Setting<R>(string middleware, string property)
		{
			return (R)Setting(middleware, property);
		}

		public dynamic Setting(string middleware, string property)
		{
			var descriptor = ComponentDescriptor.Settings(Context, middleware);

			descriptor.Validate();
			descriptor.ValidateConfiguration();

			var settings = descriptor.Context.CreateMiddleware<ISettingsMiddleware>(descriptor.Configuration.Middleware(descriptor.Context));

			if (settings == null)
				throw new RuntimeException($"{SR.ErrMiddlewareNull} ({middleware})");

			var prop = settings.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			if (prop == null)
				throw new RuntimeException($"{SR.ErrSettingPropertyNull} ({property})");

			return prop.GetValue(settings);
		}
	}
}
