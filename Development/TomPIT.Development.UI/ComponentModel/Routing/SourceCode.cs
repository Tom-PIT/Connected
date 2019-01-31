using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Text;
using TomPIT.Analysis;
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

			var fileName = string.Empty;
			var source = Connection.GetService<IComponentService>().SelectText(ms.Token, text);

			Context.Response.ContentType = "text/plain";

			if (!string.IsNullOrWhiteSpace(source))
			{
				var buffer = Encoding.UTF8.GetBytes(source);

				Context.Response.Headers.Add("Content-Disposition", string.Format("attachment;filename=\"{0}\"", text.ScriptName(Connection)));
				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.Write(buffer, 0, buffer.Length);
			}
		}
	}
}
