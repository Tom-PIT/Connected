using System;
using System.Net;

using TomPIT.Design;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Routing
{
	internal class DeployRouteHandler : RouteHandlerBase
	{
		//TODO Override from configuration
		private const string Remote = "https://sys-connected.tompit.com/rest";

		private string _baseUrl = null;

		private string BaseUrl
		{
			get
			{
				if (_baseUrl is not null)
					return _baseUrl;

				var config = Shell.Configuration;

				if (config.RootElement.TryGetProperty("DeployRouteBaseUrl", out var baseUrlNode))
					_baseUrl = baseUrlNode.GetString();

				//Reset if value makes no sense
				if (string.IsNullOrWhiteSpace(_baseUrl))
					_baseUrl = Remote;

				return _baseUrl;
			}
		}

		private DeployRouteHandler()
		{

		}

		protected override void OnProcessRequest()
		{
			var ctx = Tenant ?? MiddlewareDescriptor.Current.Tenant;

			if (!ctx.GetService<IAuthorizationService>().Demand(MiddlewareDescriptor.Current.UserToken, SecurityUtils.FullControlRole))
			{
				Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				return;
			}

			var body = Context.Request.Body.ToJObject();

			var repository = body.Required<Guid>("repository");
			var key = body.Required<string>("authenticationKey");
			var branch = body.Optional("branch", 0L);
			var commit = body.Optional("commit", 0L);

			ctx.GetService<IDesignService>().Deployment.Deploy(BaseUrl, repository, branch, commit, key);
		}
	}
}
