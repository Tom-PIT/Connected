using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Rest
{
	public class ApiHandler : ExecutionContext
	{
		public ApiHandler(HttpContext context, string endpoint) : base(context.Request, endpoint)
		{
			HttpContext = context;

			var routeData = Request.HttpContext.GetRouteData();

			MicroServiceName = routeData.Values["microService"].ToString();
			Api = routeData.Values["api"].ToString();
			Operation = routeData.Values["operation"].ToString();
		}

		private HttpContext HttpContext { get; }

		private string MicroServiceName { get; set; }
		private string Api { get; set; }
		private string Operation { get; set; }
		private IMicroService MicroService { get; set; }

		public void Invoke()
		{
			var config = GetConfiguration();

			if (config == null)
				return;

			var op = GetOperation(config);

			if (!Authorize(config, op))
				return;

			var r = Invoke<object>(string.Format("{0}/{1}", Api, Operation), ParseArguments());

			RenderResult(JsonConvert.SerializeObject(r));
		}

		private bool Authorize(IApi api, IApiOperation operation)
		{
			var e = new AuthorizationArgs(this.GetAuthenticatedUserToken(), Claims.Invoke, api.Component.ToString());

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			var r = Connection.GetService<IAuthorizationService>().Authorize(this, e);

			if (!r.Success)
			{
				if (e.User != Guid.Empty)
					RenderError((int)HttpStatusCode.Forbidden, SR.StatusForbiddenMessage);
				else
					RenderError((int)HttpStatusCode.Unauthorized, SR.StatusForbiddenMessage);

				return false;
			}

			e = new AuthorizationArgs(this.GetAuthenticatedUserToken(), Claims.Invoke, operation.Id.ToString());

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			r = Connection.GetService<IAuthorizationService>().Authorize(this, e);

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
			if (Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
				return Request.Body.ToJObject();
			else
			{
				var r = new JObject();

				foreach (var i in Request.Query.Keys)
					r.Add(i, Request.Query[i].ToString());

				return r;
			}
		}

		private void RenderError(int statusCode, string content)
		{
			HttpContext.Response.ContentType = "application/json";
			HttpContext.Response.StatusCode = statusCode;

			var json = new JObject
			{
				{ "message", content }
			};

			var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));
			HttpContext.Response.ContentLength = buffer.Length;

			HttpContext.Response.Body.Write(buffer, 0, buffer.Length);
		}

		private void RenderResult(string content)
		{
			var buffer = Encoding.UTF8.GetBytes(content);

			HttpContext.Response.Clear();
			HttpContext.Response.ContentLength = buffer.Length;
			HttpContext.Response.ContentType = "application/json";
			HttpContext.Response.StatusCode = StatusCodes.Status200OK;

			HttpContext.Response.Body.Write(buffer, 0, buffer.Length);
		}

		private IApi GetConfiguration()
		{
			MicroService = GetService<IMicroServiceService>().Select(MicroServiceName);

			if (MicroService == null)
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, MicroServiceName));

			var component = GetService<IComponentService>().SelectComponent(MicroService.Token, "Api", Api);

			if (component == null)
			{
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrComponentNotFound, Api));
				return null;
			}

			Api = component.Name;

			Initialize(Request, Endpoint, "Rest", component.Token.AsString(), MicroService.Token.AsString());

			if (!(GetService<IComponentService>().SelectConfiguration(component.Token) is IApi config))
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

		private IApiOperation GetOperation(IApi config)
		{
			var routeData = Request.HttpContext.GetRouteData();
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
						if (!Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
						{
							RenderError(StatusCodes.Status400BadRequest, string.Format("{0} ({1}/{2})", SR.ErrApiOperationProtocolGetOnly, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Post:
						if (!Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
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