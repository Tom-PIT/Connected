using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.UI;

namespace TomPIT.App.Models
{
	public class PartialModel : AjaxModel, IViewModel
	{
		public IViewConfiguration ViewConfiguration => null;

		public IModelNavigation Navigation => null;
		public string Title => null;
		public IComponent Component { get; set; }

		public ITempDataProvider TempData { get; }

		protected override void OnDatabinding()
		{
			var identifier = Body.Required<string>("__name");
			var context = FromIdentifier(identifier, MiddlewareDescriptor.Current.Tenant);

			MicroService = context.MicroService;

			var descriptor = ComponentDescriptor.Partial(this, identifier);

			descriptor.Validate();

			Component = descriptor.Component;
			QualifierName = $"{MicroService.Name}/{descriptor.ComponentName}";

			var args = new ViewInvokeArguments(this);

			Body.Remove("__name");
			Body.Remove("__component");

			Tenant.GetService<ICompilerService>().Execute(descriptor.Configuration.MicroService(), descriptor.Configuration.Invoke, this, args);
		}
	}
}
