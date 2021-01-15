using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.App.Models;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Security;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	internal class ViewEngine : ViewEngineBase, IViewEngine
	{
		public ViewEngine(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider) : base(viewEngine, tempDataProvider, serviceProvider)
		{
		}

		public async Task<string> CompilePartial(IMicroServiceContext context, string name)
		{
			var partialView = ResolveView(context, name);

			if (partialView == null)
				return null;

			using var vm = CreatePartialModel(name);
			var viewEngineResult = Engine.FindView(vm.ActionContext, name, false);
			var view = viewEngineResult.View;

			return await CreateContent(view, vm);
		}

		public async Task RenderPartial(IMicroServiceContext context, string name)
		{
			var partialView = ResolveView(context, name);

			if (partialView == null)
				return;

			using var vm = CreatePartialModel(name);
			var viewEngineResult = Engine.FindView(vm.ActionContext, name, false);
			var view = viewEngineResult.View;
			var content = await CreateContent(view, vm);

			var buffer = Encoding.UTF8.GetBytes(content);

			if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
			{
				Context.Response.Headers.Add("X-TP-VIEW", name);
				await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
			}
		}

		public async Task Render(string name)
		{
			name = name.Trim('/');

			using var model = CreateModel();

			if (model == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var metric = Guid.Empty;
			var content = string.Empty;

			if (model.ViewConfiguration != null)
				metric = model.Services.Diagnostic.StartMetric(model.ViewConfiguration.Metrics, model.ViewConfiguration.Metrics.ParseRequest(Context.Request));

			try
			{
				if (model.ViewConfiguration.AuthorizationEnabled && string.Compare(model.ViewConfiguration.Url, "login", true) != 0 && !SecurityExtensions.AuthorizeUrl(model, model.ViewConfiguration.Url))
					return;

				if (!model.ActionContext.RouteData.Values.ContainsKey("Action"))
					model.ActionContext.RouteData.Values.Add("Action", name);

				try
				{
					var viewEngineResult = Engine.FindView(model.ActionContext, name, false);

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

					content = await CreateContent(view, model);

					var buffer = Encoding.UTF8.GetBytes(content);

					if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
						await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
				}
				catch (CompilerException)
				{
					throw;
				}
				catch (Exception ex)
				{
					if (ex is NotFoundException || ex.InnerException is NotFoundException)
						Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					else
						throw new CompilerException(model.Tenant, model.ViewConfiguration, ex);
				}
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
			var view = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(path, ac);

			if (view == null)
				return null;

			ac.ActionDescriptor.Properties.Add("viewKind", ViewKind.View);

			var vi = new ViewInfo(string.Format("/Views/Dynamic/View/{0}.cshtml", path), ac);
			var ms = vi.ViewComponent == null ? null : MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(vi.ViewComponent.MicroService);

			var model = new RuntimeModel(Context.Request, ac, Temp, ms)
			{
				ViewConfiguration = view,
			};

			model.Initialize(null, ms);

			if (model is IComponentModel cm && vi.ViewComponent != null)
				cm.Component = vi.ViewComponent;

			return model;
		}

		private RuntimeModel CreatePartialModel(string name)
		{
			var ac = CreateActionContext(Context);

			ac.ActionDescriptor.Properties.Add("viewKind", ViewKind.Partial);

			var vi = new ViewInfo(string.Format("/Views/Dynamic/Partial/{0}.cshtml", name), ac);
			var ms = vi.ViewComponent == null ? null : MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(vi.ViewComponent.MicroService);

			var model = new RuntimeModel(Context.Request, ac, Temp, ms);

			model.Initialize(null, ms);

			if (model is IComponentModel cm && vi.ViewComponent != null)
				cm.Component = vi.ViewComponent;

			return model;
		}

		private IPartialViewConfiguration ResolveView(IMicroServiceContext context, string qualifier)
		{
			var tokens = qualifier.Split('/');
			var ms = context.MicroService;
			var name = qualifier;

			if (tokens.Length > 1)
			{
				ms = context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
					return null;

				name = tokens[1];
			}

			return context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "Partial", name) as IPartialViewConfiguration;
		}
	}
}