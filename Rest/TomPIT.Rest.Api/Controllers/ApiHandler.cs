using System;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.Rest.Controllers
{
	public class ApiHandler : MicroServiceContext
	{
		public ApiHandler(HttpContext context)
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

			var r = Interop.Invoke<object, JObject>(string.Format("{0}/{1}", Api, Operation), ParseArguments());

			if (Shell.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK)
			{
				var type = Tenant.GetService<ICompilerService>().ResolveType(config.MicroService(), op, op.Name, false);

				if (type != null && type.ImplementsInterface(typeof(IDistributedOperation)))
					Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
				else
					RenderResult(JsonConvert.SerializeObject(r));
			}
		}

		private bool Authorize(IApiConfiguration api, IApiOperation operation)
		{
			var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(api.Component);
			var e = new AuthorizationArgs(MiddlewareDescriptor.Current.UserToken, Claims.Invoke, api.Component.ToString());

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

			Shell.HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
		}

		private void RenderResult(string content)
		{
			var buffer = Encoding.UTF8.GetBytes(content);

			Shell.HttpContext.Response.Clear();
			Shell.HttpContext.Response.ContentLength = buffer.Length;
			Shell.HttpContext.Response.ContentType = "application/json";
			Shell.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

			Shell.HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
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

			Initialize(Endpoint);

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
				RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrApiOperationProtocolRestDisabled, Api, Operation));
				return null;
			}

			if (op.Protocols.RestVerbs != ApiOperationVerbs.All)
			{
				switch (op.Protocols.RestVerbs)
				{
					case ApiOperationVerbs.Get:
						if (!Shell.HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Post:
						if (!Shell.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}
						break;
					case ApiOperationVerbs.Patch:
						if (!Shell.HttpContext.Request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Delete:
						if (!Shell.HttpContext.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Put:
						if (!Shell.HttpContext.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Head:
						if (!Shell.HttpContext.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
				}
			}

			if (op.Scope != ElementScope.Public)
			{
				RenderError((int)HttpStatusCode.NotFound, SR.ErrScopeError);
				return null;
			}

			return op;
		}

		private void SetAllowHeader(ApiOperationVerbs verbs)
		{
			var sb = new StringBuilder();

			if ((verbs & ApiOperationVerbs.Get) == ApiOperationVerbs.Get)
				sb.Append($"GET,");

			if ((verbs & ApiOperationVerbs.Head) == ApiOperationVerbs.Head)
				sb.Append($"HEAD,");

			if ((verbs & ApiOperationVerbs.Post) == ApiOperationVerbs.Post)
				sb.Append($"POST,");

			if ((verbs & ApiOperationVerbs.Put) == ApiOperationVerbs.Put)
				sb.Append($"PUT,");

			if ((verbs & ApiOperationVerbs.Delete) == ApiOperationVerbs.Delete)
				sb.Append($"DELETE,");

			if ((verbs & ApiOperationVerbs.Patch) == ApiOperationVerbs.Patch)
				sb.Append($"PATCH,");

			Shell.HttpContext.Response.Headers.Add(Enum.GetName(typeof(HttpResponseHeader), HttpResponseHeader.Allow), sb.ToString().TrimEnd(','));
		}
	}
}