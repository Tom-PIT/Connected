using System;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Rest.Controllers
{
	public class ApiHandler : MiddlewareContext
	{
		public ApiHandler(HttpContext context, string endpoint) : base(endpoint)
		{
			var routeData = Shell.HttpContext.GetRouteData();

			MicroServiceName = routeData.Values["microService"].ToString();
			Api = routeData.Values["api"].ToString();
			Operation = routeData.Values["operation"].ToString();
		}

		private string MicroServiceName { get; set; }
		private string Api { get; set; }
		private string Operation { get; set; }

		public void Invoke()
		{
			var config = GetConfiguration();

			if (config == null)
				return;

			var op = GetOperation(config);

			if (!Authorize(config, op))
				return;

			var r = Invoke<object, JObject>(string.Format("{0}/{1}", Api, Operation), ParseArguments());

			if (Shell.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK)
				RenderResult(JsonConvert.SerializeObject(r));
		}

		private bool Authorize(IApiConfiguration api, IApiOperation operation)
		{
			var component = Instance.Tenant.GetService<IComponentService>().SelectComponent(api.Component);
			var e = new AuthorizationArgs(MiddlewareDescriptor.Current.UserToken, Claims.Invoke, api.Component.ToString(), component.Folder);

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			var r = Tenant.GetService<IAuthorizationService>().Authorize(this, e);

			if (!r.Success)
			{
				if (e.User != Guid.Empty)
					RenderError((int)HttpStatusCode.Forbidden, SR.StatusForbiddenMessage);
				else
					RenderError((int)HttpStatusCode.Unauthorized, SR.StatusForbiddenMessage);

				return false;
			}

			e = new AuthorizationArgs(MiddlewareDescriptor.Current.UserToken, Claims.Invoke, operation.Id.ToString());

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			r = Tenant.GetService<IAuthorizationService>().Authorize(this, e);

			if (!r.Success)
			{
				if (r.Reason == AuthorizationResultReason.Empty)
					return true;

				if (e.User != Guid.Empty)
					RenderError((int)HttpStatusCode.Forbidden, SR.StatusForbiddenMessage);
				else
					RenderError((int)HttpStatusCode.Unauthorized, SR.StatusForbiddenMessage);

				return false;
			}

			return true;
		}

		private JObject ParseArguments()
		{
			if (Shell.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
				return Shell.HttpContext.Request.Body.ToJObject();
			else
			{
				var r = new JObject();

				foreach (var i in Shell.HttpContext.Request.Query.Keys)
					r.Add(i, Shell.HttpContext.Request.Query[i].ToString());

				return r;
			}
		}

		private void RenderError(int statusCode, string content)
		{
			Shell.HttpContext.Response.ContentType = "application/json";
			Shell.HttpContext.Response.StatusCode = statusCode;

			var json = new JObject
			{
				{ "message", content }
			};

			var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));
			Shell.HttpContext.Response.ContentLength = buffer.Length;

			Shell.HttpContext.Response.Body.Write(buffer, 0, buffer.Length);
		}

		private void RenderResult(string content)
		{
			var buffer = Encoding.UTF8.GetBytes(content);

			Shell.HttpContext.Response.Clear();
			Shell.HttpContext.Response.ContentLength = buffer.Length;
			Shell.HttpContext.Response.ContentType = "application/json";
			Shell.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

			Shell.HttpContext.Response.Body.Write(buffer, 0, buffer.Length);
		}

		private IApiConfiguration GetConfiguration()
		{
			MicroService = Tenant.GetService<IMicroServiceService>().Select(MicroServiceName);

			if (MicroService == null)
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, MicroServiceName));

			var component = Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, "Api", Api);

			if (component == null)
			{
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrComponentNotFound, Api));
				return null;
			}

			Api = component.Name;

			Initialize(Endpoint, MicroService);

			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IApiConfiguration config))
			{
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrCannotFindConfiguration, Api));
				return null;
			}

			if (!config.Protocols.Rest)
			{
				RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1})", SR.ErrApiProtocolRestDisabled, Api));
				return null;
			}

			if (config.Scope != ElementScope.Public)
			{
				RenderError((int)HttpStatusCode.MethodNotAllowed, SR.ErrScopeError);
				return null;
			}

			return config;
		}

		private IApiOperation GetOperation(IApiConfiguration config)
		{
			var routeData = Shell.HttpContext.GetRouteData();
			var operation = routeData.Values["operation"].ToString();

			var op = config.Operations.FirstOrDefault(f => string.Equals(f.Name, operation, System.StringComparison.OrdinalIgnoreCase));

			if (op == null)
			{
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrServiceOperationNotFound, Api, Operation));
				return null;
			}

			Operation = op.Name;

			if (!op.Protocols.Rest)
			{
				RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ErrApiOperationProtocolRestDisabled, Api, Operation));
				return null;
			}

			if (op.Protocols.RestVerbs != ApiOperationVerbs.All)
			{
				switch (op.Protocols.RestVerbs)
				{
					case ApiOperationVerbs.Get:
						if (!Shell.HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
						{
							RenderError(StatusCodes.Status400BadRequest, string.Format("{0} ({1}/{2})", SR.ErrApiOperationProtocolGetOnly, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Post:
						if (!Shell.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
						{
							RenderError(StatusCodes.Status400BadRequest, string.Format("{0} ({1}/{2})", SR.ErrApiOperationProtocolPostOnly, Api, Operation));
							return null;
						}

						break;
				}
			}

			if (op.Scope != ElementScope.Public)
			{
				RenderError((int)HttpStatusCode.MethodNotAllowed, SR.ErrScopeError);
				return null;
			}

			return op;
		}
	}
}