using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Middleware;
using TomPIT.Models;

namespace TomPIT.App.Models
{
	public class PartialModel : AjaxModel, IViewModel
	{
		public IViewConfiguration ViewConfiguration => null;

		public IModelNavigation Navigation => null;
		public string Title => null;
		public IComponent Component { get; set; }

		public ITempDataProvider TempData { get; }

		public override IRuntimeModel Clone() 
		{
			var model = new PartialModel() 
			{
				Body = (JObject)Body.DeepClone(),
				QualifierName = QualifierName,
				Component = Component,
			};

			model.Initialize(Controller, MicroService);

			return model;
		}

        protected override void OnDatabinding()
		{
			var identifier = Body.Required<string>("__name");
			var context = FromIdentifier(identifier, MiddlewareDescriptor.Current.Tenant);

			MicroService = context.MicroService;

			var descriptor = ComponentDescriptor.Partial(this, identifier);

			descriptor.Validate();

			Component = descriptor.Component;
			QualifierName = $"{MicroService.Name}/{descriptor.ComponentName}";

			Body.Remove("__name");
			Body.Remove("__component");
		}
	}
}
