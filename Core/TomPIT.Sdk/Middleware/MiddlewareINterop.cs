using System;
using TomPIT.ComponentModel;
using TomPIT.Middleware.Interop;
using TomPIT.Serialization;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware
{
	internal class MiddlewareInterop : MiddlewareObject, IMiddlewareInterop
	{
		public MiddlewareInterop(IMiddlewareContext context) : base(context)
		{

		}
		public R Invoke<R>([CAP(CAP.ApiProvider)]string api)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);
			var result = invoker.Invoke(this as IApiExecutionScope, descriptor, EventArgs.Empty);

			return Marshall.Convert<R>(result);
		}

		public R Invoke<R, A>([CAP(CAP.ApiProvider)] string api, A e)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);
			var result = invoker.Invoke(this as IApiExecutionScope, descriptor, e);

			return Marshall.Convert<R>(result);
		}

		public void Invoke<A>([CAP(CAP.ApiProvider)] string api, A e)
		{
			var descriptor = ComponentDescriptor.Api(Context, api);
			var invoker = new ApiInvoker(Context);

			invoker.Invoke(this as IApiExecutionScope, descriptor, e);
		}
	}
}
