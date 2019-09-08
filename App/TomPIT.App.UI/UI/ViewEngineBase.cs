using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using TomPIT.ComponentModel.UI;

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

		protected string CreateContent(Microsoft.AspNetCore.Mvc.ViewEngines.IView view, ViewInvokeArguments e)
		{
			using (var output = new StringWriter())
			{
				var viewContext = new ViewContext(e.Model.ActionContext, view, e.ViewData, e.TempData, output, new HtmlHelperOptions());

				view.RenderAsync(viewContext).GetAwaiter().GetResult();

				return output.ToString();
			}
		}
	}
}
