using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Design;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.Design.Designers
{
	internal class SynchronizeEntityToolbarAction : IAmbientToolbarAction
	{
		public string Glyph => "fal fa-sync";

		public string Text => "Synchronize Entity";

		public string Action => "SynchronizeEntity";

		public void Invoke(IMiddlewareContext context, IElement element)
		{
			context.Tenant.GetService<IModelService>().SynchronizeEntity(element.Configuration() as IModelConfiguration);
		}
	}
}
