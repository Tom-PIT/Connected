using System.Text;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design;
using TomPIT.Ide.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Development.Handlers
{
	internal class ApiOperationCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IMiddlewareContext context, object instance)
		{
			if (instance is IApiOperation operation)
			{
				var sb = new StringBuilder();

				sb.AppendLine($"using System.ComponentModel.DataAnnotations;");
				sb.AppendLine($"using TomPIT.Annotations;");
				sb.AppendLine("");
				sb.AppendLine($"public class {operation.Name} : Operation");
				sb.AppendLine("{");
				sb.AppendLine("");
				sb.AppendLine("");
				sb.AppendLine("}");

				context.Tenant.GetService<IComponentDevelopmentService>().Update(operation, sb.ToString());
			}
		}
	}
}
