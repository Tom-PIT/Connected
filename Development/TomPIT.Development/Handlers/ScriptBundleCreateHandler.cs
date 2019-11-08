using TomPIT.ComponentModel.Resources;
using TomPIT.Design;
using TomPIT.Ide.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Development.Handlers
{
	internal class ScriptBundleCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IMiddlewareContext context, object instance)
		{
			if (instance is IScriptBundleConfiguration script)
			{
				if (script is IScriptBundleInitializer initializer)
					script.Scripts.Add(initializer.CreateDefaultFile());

				context.Tenant.GetService<IComponentDevelopmentService>().Update(script);
			}
		}
	}
}
