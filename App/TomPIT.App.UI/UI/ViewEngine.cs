using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TomPIT.App.Models;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	internal class ViewEngine : ViewEngineBase, IViewEngine
	{
		private List<IRuntimeViewModifier> ViewModifiers => MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceRuntimeService>()
			 .QueryRuntimes()
			 .Select(e => e?.ViewModifier)
			 .Where(e => e is not null)
			 .OrderBy(e => e.Priority)
			 .ToList();

		public ViewEngine(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider) : base(viewEngine, tempDataProvider, serviceProvider)
		{
		}

		public async Task<string> RenderPartialToStringAsync(IMicroServiceContext context, string name)
		{
			var partialView = ResolveView(context, name);

			if (partialView is null)
				return null;

			using var vm = CreatePartialModel(name);
			var viewEngineResult = Engine.FindView(vm.ActionContext, name, false);
			var view = viewEngineResult.View;

			var content = await CreateContent(view, vm);

			var modifiers = this.ViewModifiers;

			if (!modifiers.Any())
				return content;

			var postRenderArgs = new PartialViewPostRenderModificationArguments
			{
				Arguments = vm.Arguments,
				Component = vm.Component,
				MicroService = vm.MicroService,
				Configuration = vm.ViewConfiguration as IPartialViewConfiguration,
				Name = name,
				Content = content
			};

			foreach (var modifier in modifiers)
			{
				postRenderArgs.Content = modifier.PostRenderPartialView(postRenderArgs);
			}

			return postRenderArgs.Content;
		}

		public async Task RenderPartial(IMicroServiceContext context, string name)
		{
			var content = await RenderPartialToStringAsync(context, name);

			var buffer = Encoding.UTF8.GetBytes(content);

			if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
			{
				Context.Response.Headers.Add("X-TP-VIEW", name);
				await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
			}

			await Context.Response.CompleteAsync();
		}

		public async Task Render(string name)
		{
			name = name.Trim('/');

			using var model = CreateModel();

			if (model is null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var content = string.Empty;

			try
			{
				var user = model.Services.Identity.IsAuthenticated ? model.Services.Identity.User.Token : Guid.Empty;
				if (model.ViewConfiguration.AuthorizationEnabled && string.Compare(model.ViewConfiguration.Url, "login", true) != 0 && !SecurityExtensions.AuthorizeUrl(model, model.ViewConfiguration.Url, user))
					return;

				if (!model.ActionContext.RouteData.Values.ContainsKey("Action"))
					model.ActionContext.RouteData.Values.Add("Action", name);

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

				var modifiers = this.ViewModifiers;

				if (modifiers.Any())
				{
					var postRenderArgs = new ViewPostRenderModificationArguments
					{
						Arguments = model.Arguments,
						Component = model.Component,
						MicroService = model.MicroService,
						Configuration = model.ViewConfiguration,
						Name = $"{model.MicroService.Name}/{model.Component.Name}",
						Url = name,
						Content = content
					};

					foreach (var modifier in modifiers)
					{
						postRenderArgs.Content = modifier.PostRenderView(postRenderArgs);
					}

					content = postRenderArgs.Content;
				}

				var buffer = Encoding.UTF8.GetBytes(content);

				if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
				{
					Context.Response.ContentType = "text/html; charset=UTF-8";
					await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
				}

				await Context.Response.CompleteAsync();
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
					throw new CompilerException(model.ViewConfiguration, ex);
			}
		}

		private RuntimeModel CreateModel()
		{
			var path = Context.Request.Path.ToString().Trim('/');

			var ac = CreateActionContext(Context);
			var view = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(path, ac);

			if (view is null)
				return null;

			ac.ActionDescriptor.Properties.Add("viewKind", ViewKind.View);

			var vi = new ViewInfo(string.Format("/Views/Dynamic/View/{0}.cshtml", path), ac);
			var ms = vi.ViewComponent is null ? null : MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(vi.ViewComponent.MicroService);

			var model = new RuntimeModel(Context.Request, ac, Temp, ms)
			{
				ViewConfiguration = view,
			};

			model.Initialize(null, ms);

			if (model is IComponentModel cm && vi.ViewComponent != null)
				cm.Component = vi.ViewComponent;

			var modifiers = this.ViewModifiers;

			if (modifiers.Any())
			{
				var preRenderArgs = new ViewPreRenderModificationArguments
				{
					Arguments = model.Arguments,
					Component = model.Component,
					MicroService = model.MicroService,
					Configuration = model.ViewConfiguration,
					Name = $"{model.MicroService.Name}/{model.Component.Name}",
					Url = path
				};

				foreach (var modifier in modifiers)
				{
					preRenderArgs = modifier.PreRenderView(preRenderArgs);
				}

				model.ReplaceArguments(preRenderArgs.Arguments);
			}


			return model;
		}

		private RuntimeModel CreatePartialModel(string name)
		{
			var ac = CreateActionContext(Context);

			ac.ActionDescriptor.Properties.Add("viewKind", ViewKind.Partial);

			var vi = new ViewInfo(string.Format("/Views/Dynamic/Partial/{0}.cshtml", name), ac);
			var ms = vi.ViewComponent is null ? null : MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(vi.ViewComponent.MicroService);

			var model = new RuntimeModel(Context.Request, ac, Temp, ms);

			var body = Context.Request.Body.ToJObject();

			model.Arguments.Merge(body);

			model.Databind();

			model.Initialize(null, ms);

			var modifiers = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceRuntimeService>()
			.QueryRuntimes()
			.Select(e => e?.ViewModifier)
			.Where(e => e is not null)
			.OrderBy(e => e.Priority)
			.ToList();

			if (modifiers.Any())
			{
				var microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(name.Split('/').FirstOrDefault());
				var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(microService.Token, "Partial", name.Split('/').LastOrDefault());
				var configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

				var preRenderArgs = new PartialViewPreRenderModificationArguments
				{
					Arguments = model.Arguments,
					Component = component,
					MicroService = microService,
					Configuration = configuration as IPartialViewConfiguration,
					Name = name
				};

				foreach (var modifier in modifiers)
				{
					preRenderArgs = modifier.PreRenderPartialView(preRenderArgs);
				}

				model.ReplaceArguments(preRenderArgs.Arguments);
			}

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

				if (ms is null)
					return null;

				name = tokens[1];
			}

			var partialResolutionArgs = new PartialViewResolutionArgs
			{
				Name = qualifier
			};

			foreach (var runtime in context.Tenant.GetService<IMicroServiceRuntimeService>().QueryRuntimes())
			{
				if (runtime?.Resolver?.ResolvePartial(partialResolutionArgs) is IPartialViewConfiguration config)
					return config;
			}

			return context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "Partial", name) as IPartialViewConfiguration;
		}
	}
}