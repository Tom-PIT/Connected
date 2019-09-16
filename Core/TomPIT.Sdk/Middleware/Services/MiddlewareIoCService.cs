using TomPIT.Compilation;
using TomPIT.ComponentModel;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareIoCService : MiddlewareComponent, IMiddlewareIoCService
	{
		public MiddlewareIoCService(IMiddlewareContext context) : base(context)
		{
		}

		public IMiddlewareIoC UseMiddleware(string type)
		{
			var targetDescriptor = ComponentDescriptor.Script(Context, type);

			targetDescriptor.Validate();

			return Context.Tenant.GetService<ICompilerService>().CreateInstance<IMiddlewareIoC>(targetDescriptor.Configuration);
		}
	}
}
