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
using System.Net;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Models;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.UI
{
	internal class ViewEngine : IViewEngine
	{
		public ViewEngine(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider)
		{
			Engine = viewEngine;
			Temp = tempDataProvider;
			ServiceProvider = serviceProvider;
		}

		private IRazorViewEngine Engine { get; }
		private ITempDataProvider Temp { get; }
		private System.IServiceProvider ServiceProvider { get; }

		public HttpContext Context { get; set; }

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
			var metric = Guid.Empty;
			var content = string.Empty;

			if (model.View != null)
				metric = model.Services.Diagnostic.StartMetric(model.View.Metrics, model.View.Metrics.ParseRequest(Context.Request));

			try
			{
				var actionContext = GetActionContext(Context);
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

				Authorize(model);

				if (Context.Response.StatusCode != (int)HttpStatusCode.OK)
					return;

				content = CreateContent(view, actionContext, model);
				var buffer = Encoding.UTF8.GetBytes(content);

				if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
					Context.Response.Body.Write(buffer, 0, buffer.Length);
			}
			finally
			{
				model.Services.Diagnostic.StopMetric(metric, model.View.Metrics.ParseResponse(Context.Response, content));
			}
		}

		private string CreateContent<TModel>(Microsoft.AspNetCore.Mvc.ViewEngines.IView view, ActionContext actionContext, TModel model)
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

		internal static ActionContext GetActionContext(HttpContext context)
		{
			return new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
		}

		private RuntimeModel CreateModel()
		{
			var path = Context.Request.Path.ToString().Trim('/');

			var ac = GetActionContext(Context);
			var view = Instance.GetService<IViewService>().Select(path, ac);

			if (view == null)
				return null;

			var model = new RuntimeModel(Context.Request, ac);

			model.View = view;

			var vi = new ViewInfo(string.Format("/Views/{0}.cshtml", path), ac);

			if (model is IIdentityBinder ctx && vi.ViewComponent != null)
				ctx.Bind(vi.ViewComponent.Token.ToString(), vi.ViewComponent.Category, vi.ViewComponent.MicroService.ToString());

			if (model is IComponentModel cm && vi.ViewComponent != null)
				cm.Component = vi.ViewComponent;

			return model;
		}

		private void Authorize(IExecutionContext model)
		{
			var rt = model as RuntimeModel;

			if (rt.Component == null)
				return;

			Authorize(rt, rt.Component);
		}

		private void Authorize(RuntimeModel model, IComponent component)
		{
			var e = new AuthorizationArgs(model.GetAuthenticatedUserToken(), Claims.AccessUserInterface, component.Token.ToString());

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			var r = model.Connection().GetService<IAuthorizationService>().Authorize(model, e);

			if (r.Success)
				return;

			if (r.Reason == AuthorizationResultReason.Empty)
			{
				if (model.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IAuthorizationChain view
					&& view.AuthorizationParent != Guid.Empty)
				{
					var c = model.Connection().GetService<IComponentService>().SelectComponent(view.AuthorizationParent);

					if (c != null)
						Authorize(model, c);
				}
				else
					Reject(model);
			}
			else
				Reject(model);
		}

		private void Reject(IExecutionContext context)
		{
			if (context.GetAuthenticatedUserToken() == Guid.Empty)
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			else
				Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}
	}
}