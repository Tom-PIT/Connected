using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Routing;
using TomPIT.Annotations.Design;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Routing;
using TomPIT.Security;

namespace TomPIT.Development.Routing
{
	internal class SourceCode : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var microService = new Guid(Context.GetRouteValue("microService").ToString());

			if (Tenant == null || User == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}

			var aa = new AuthorizationArgs(User.Token, Claims.ImplementMicroservice, microService.ToString());

			aa.Schema.Empty = EmptyBehavior.Deny;
			aa.Schema.Level = AuthorizationLevel.Pessimistic;

			if (!Tenant.GetService<IAuthorizationService>().Authorize(new MiddlewareContext(Tenant.Url, Tenant.GetService<IMicroServiceService>().Select(microService)), aa).Success)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var component = Tenant.GetService<IComponentService>().SelectComponent(new Guid(Context.GetRouteValue("component").ToString()));

			if (component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var element = Tenant.GetService<IDiscoveryService>().Find(component.Token, new Guid(Context.GetRouteValue("template").ToString()));

			if (element == null || !(element is IText text))
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var fileName = text.ScriptName(Tenant);
			var source = LoadSource(ms.Token, text);

			Context.Response.ContentType = "text/plain";

			if (!string.IsNullOrWhiteSpace(source))
			{
				var buffer = Encoding.UTF8.GetBytes(source);

				Context.Response.Headers.Add("Content-Disposition", string.Format("attachment;filename=\"{0}\"", fileName));
				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.Write(buffer, 0, buffer.Length);
			}
		}

		private string LoadSource(Guid microService, IText text)
		{
			var att = text.GetType().FindAttribute<SyntaxAttribute>();

			if (att == null)
				return LoadScriptSource(microService, text);
			else if (string.Compare(att.Syntax, SyntaxAttribute.Razor, true) == 0)
				return LoadRazorSource(microService, text);

			return LoadScriptSource(microService, text);
		}

		private string LoadScriptSource(Guid microService, IText text)
		{
			return Tenant.GetService<IComponentService>().SelectText(microService, text);
		}

		private string LoadRazorSource(Guid microService, IText text)
		{
			return Tenant.GetService<ICompilerService>().CompileView(Tenant, text as ISourceCode);
		}
	}
}
