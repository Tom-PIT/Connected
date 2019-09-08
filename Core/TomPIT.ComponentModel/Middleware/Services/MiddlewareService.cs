using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Data;
using TomPIT.Middleware;
using TomPIT.Services;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareService : MiddlewareComponent, IMiddlewareService
	{
		public MiddlewareService(IDataModelContext context) : base(context)
		{
		}

		public IMiddlewareEntity CreateInstance(string type)
		{
			var targetDescriptor = ComponentDescriptor.Script(new DataModelContext(Context), type);

			targetDescriptor.Validate();

			return Context.Connection().GetService<ICompilerService>().CreateInstance<IMiddlewareEntity>(targetDescriptor.Configuration);
		}
	}
}
