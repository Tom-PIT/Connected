using Microsoft.AspNetCore.Routing;
using System;
using System.Net;
using System.Text;
using TomPIT.Analysis;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.Compilers;
using TomPIT.Routing;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Routing
{
	internal class SourceCode : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var microService = Context.GetRouteValue("microService").ToString().AsGuid();

			if (Connection == null || User == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}

			var aa = new AuthorizationArgs(User.Token, Claims.ImplementMicroservice, microService.ToString());

			aa.Schema.Empty = EmptyBehavior.Deny;
			aa.Schema.Level = AuthorizationLevel.Pessimistic;

			if (!Connection.GetService<IAuthorizationService>().Authorize(new ExecutionContext(Connection.Url), aa).Success)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}

			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var component = Connection.GetService<IComponentService>().SelectComponent(Context.GetRouteValue("component").ToString().AsGuid());

			if (component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var element = Connection.GetService<IDiscoveryService>().Find(component.Token, Context.GetRouteValue("template").ToString().AsGuid());

			if (element == null || !(element is IText text))
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var fileName = text.ScriptName(Connection);
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
			return Connection.GetService<IComponentService>().SelectText(microService, text);
		}

		private string LoadRazorSource(Guid microService, IText text)
		{
			return Connection.GetService<ICompilerService>().CompileView(Connection, text as ISourceCode);
		}
	}
}
