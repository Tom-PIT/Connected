using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Exceptions;
using TomPIT.IoC;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareIoCService : MiddlewareComponent, IMiddlewareIoCService
	{
		public MiddlewareIoCService(IMiddlewareContext context) : base(context)
		{
		}

		public R Invoke<R>(string middlewareOperation)
		{
			return Context.Tenant.GetService<IIoCService>().Invoke<R>(ResolveOperation(middlewareOperation));
		}

		public void Invoke(string middlewareOperation)
		{
			Context.Tenant.GetService<IIoCService>().Invoke(ResolveOperation(middlewareOperation));
		}

		public R Invoke<R>(string middlewareOperation, object e)
		{
			return Context.Tenant.GetService<IIoCService>().Invoke<R>(ResolveOperation(middlewareOperation), e);
		}

		public void Invoke(string middlewareOperation, object e)
		{
			Context.Tenant.GetService<IIoCService>().Invoke(ResolveOperation(middlewareOperation), e);
		}

		private IIoCOperation ResolveOperation(string middlewareOperation)
		{
			var descriptor = ComponentDescriptor.IoCContainer(Context, middlewareOperation);

			descriptor.Validate();

			var operation = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(descriptor.Element, f.Name, true) == 0);

			if (operation == null)
				throw new RuntimeException($"{SR.ErrIoCOperationNotFound} ({middlewareOperation})");

			return operation;
		}
	}
}
