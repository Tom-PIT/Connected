using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design;
using TomPIT.Services;

namespace TomPIT.Development.Handlers
{
	internal class ApiOperationCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IExecutionContext context, object instance)
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
				sb.AppendLine($"public {operation.Name} (IDataModelContext context) : base (context)");
				sb.AppendLine("{");
				sb.AppendLine("");
				sb.AppendLine("}");
				sb.AppendLine("");
				sb.AppendLine("}");

				context.Connection().GetService<IComponentDevelopmentService>().Update(operation, sb.ToString());
			}
		}
	}
}
