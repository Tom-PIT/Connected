using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.IO;
using System.Threading.Tasks;
using TomPIT.Models;

namespace TomPIT.App.UI
{
	public abstract class ViewEngineBase
	{
		protected ViewEngineBase(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider)
		{
			Engine = viewEngine;
			Temp = tempDataProvider;
			ServiceProvider = serviceProvider;
		}

		protected IRazorViewEngine Engine { get; }
		protected ITempDataProvider Temp { get; }
		protected System.IServiceProvider ServiceProvider { get; }
		public HttpContext Context { get; set; }

		internal static ActionContext CreateActionContext(HttpContext context)
		{
			return new ActionContext(context, context?.GetRouteData(), new ActionDescriptor());
		}

		protected static async Task<string> CreateContent(Microsoft.AspNetCore.Mvc.ViewEngines.IView view, IViewModel model)
		{
			var temp = new TempDataDictionary(model.ActionContext.HttpContext, model.TempData);
			var viewData = new ViewDataDictionary<IRuntimeModel>(metadataProvider: new EmptyModelMetadataProvider(), modelState: new ModelStateDictionary())
			{
				Model = model
			};

			using var output = new StringWriter();
			var viewContext = new ViewContext(model.ActionContext, view, viewData, temp, output, new HtmlHelperOptions());

			await view.RenderAsync(viewContext);

			return output.ToString();
		}
	}
}
