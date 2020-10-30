using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Design;
using TomPIT.Middleware;

namespace TomPIT.Development.Handlers
{
	internal class ScriptCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IMiddlewareContext context, object instance)
		{
			if (instance is IScriptConfiguration script)
			{
				var sb = new StringBuilder();

				sb.AppendLine($"public class {script.ComponentName()}");
				sb.AppendLine("{");
				sb.AppendLine("");
				sb.AppendLine("}");

				context.Tenant.GetService<IDesignService>().Components.Update(script, sb.ToString());
			}
		}
	}
}
