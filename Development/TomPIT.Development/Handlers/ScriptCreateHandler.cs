using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Services;

namespace TomPIT.Development.Handlers
{
	internal class ScriptCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IExecutionContext context, object instance)
		{
			if (instance is IScript script)
			{
				var sb = new StringBuilder();

				sb.AppendLine($"public class {script.ComponentName(context.Connection())}");
				sb.AppendLine("{");
				sb.AppendLine("");
				sb.AppendLine("}");

				context.Connection().GetService<IComponentDevelopmentService>().Update(script, sb.ToString());
			}
		}
	}
}
