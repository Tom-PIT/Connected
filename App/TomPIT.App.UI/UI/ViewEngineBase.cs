using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.IO;

namespace TomPIT.UI
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

		protected string CreateContent<TModel>(Microsoft.AspNetCore.Mvc.ViewEngines.IView view, ActionContext actionContext, TModel model)
		{
			using (var output = new StringWriter())
			{
				var viewData = new ViewDataDictionary<TModel>(
						  metadataProvider: new EmptyModelMetadataProvider(),
						  modelState: new ModelStateDictionary())
				{
					Model = model
				};

				var httpContextAccessor = new HttpContextAccessor
				{
					HttpContext = actionContext.HttpContext
				};

				var tempData = new TempDataDictionary(httpContextAccessor.HttpContext, Temp);
				var viewContext = new ViewContext(actionContext, view, viewData, tempData, output, new HtmlHelperOptions());

				view.RenderAsync(viewContext).GetAwaiter().GetResult();

				return output.ToString();
			}
		}
	}
}
