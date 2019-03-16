using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Net;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;
using TomPIT.Models;

namespace TomPIT.UI
{
	internal class ViewEngine : ViewEngineBase, IViewEngine
	{
		public ViewEngine(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider) : base(viewEngine, tempDataProvider, serviceProvider)
		{
		}

		public string SnippetPath(ViewContext context, string snippetName)
		{
			var vi = new ViewInfo(context.ExecutingFilePath, null);

			switch (vi.Kind)
			{
				case ViewKind.Master:
					return string.Format("~/Views/Dynamic/Snippet/Master/{0}.{1}.cshtml", vi.Path, snippetName);
				case ViewKind.View:
					return string.Format("~/Views/Dynamic/Snippet/View/{0}.{1}.cshtml", vi.Path, snippetName);
				case ViewKind.Partial:
					return string.Format("~/Views/Dynamic/Snippet/Partial/{0}.{1}.cshtml", vi.Path, snippetName);
				default:
					throw new NotSupportedException();
			}
		}

		public void Render(string name)
		{
			name = name.Trim('/');

			var model = CreateModel();

			if (model == null)
				throw new RuntimeException(SR.ErrNoViews);

			var metric = Guid.Empty;
			var content = string.Empty;

			if (model.ViewConfiguration != null)
				metric = model.Services.Diagnostic.StartMetric(model.ViewConfiguration.Metrics, model.ViewConfiguration.Metrics.ParseRequest(Context.Request));

			try
			{
				if (!model.Authorize(model.Component))
					return;

				var actionContext = CreateActionContext(Context);

				var viewEngineResult = Engine.FindView(actionContext, name, false);

				if (!viewEngineResult.Success)
				{
					if (string.Compare(name, "home", true) == 0)
						throw new InvalidOperationException(SR.ErrDefaultViewNotSet);
					else
					{
						Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
						return;
					}
				}

				var view = viewEngineResult.View;

				if (Context.Response.StatusCode != (int)HttpStatusCode.OK)
					return;

				content = CreateContent(view, actionContext, model);

				var buffer = Encoding.UTF8.GetBytes(content);

				if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
					Context.Response.Body.Write(buffer, 0, buffer.Length);
			}
			finally
			{
				model.Services.Diagnostic.StopMetric(metric, Context.Response.StatusCode < 400 ? SessionResult.Success : SessionResult.Fail, model.ViewConfiguration.Metrics.ParseResponse(Context.Response, content));
			}
		}

		private RuntimeModel CreateModel()
		{
			var path = Context.Request.Path.ToString().Trim('/');

			var ac = CreateActionContext(Context);
			var view = Instance.GetService<IViewService>().Select(path, ac);

			if (view == null)
				return null;

			var vi = new ViewInfo(string.Format("/Views/{0}.cshtml", path), ac);
			var ms = vi.ViewComponent == null ? null : Instance.GetService<IMicroServiceService>().Select(vi.ViewComponent.MicroService);

			var model = new RuntimeModel(Context.Request, ac)
			{
				ViewConfiguration = view
			};

			model.Initialize(null, ms);

			if (model is IComponentModel cm && vi.ViewComponent != null)
				cm.Component = vi.ViewComponent;

			return model;
		}
	}
}