using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.Rest.Controllers
{
	public class ApiHandler : MicroServiceContext
	{
		private ApiFormatter _formatter = null;
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

		public async Task Invoke()
		{
			var config = await GetConfiguration();

			if (config == null)
				return;

			var op = await GetOperation(config);

			if (!await Authorize(config, op))
				return;

			var r = Interop.Invoke<object, JObject>(string.Format("{0}/{1}", Api, Operation), ParseArguments());

			if (Shell.HttpContext.Response.StatusCode == (int)HttpStatusCode.OK)
			{
				var type = Tenant.GetService<ICompilerService>().ResolveType(config.MicroService(), op, op.Name, false);

				if (type != null && type.ImplementsInterface(typeof(IDistributedOperation)))
					Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
				else
					await RenderResult(r);
			}
		}

		private async Task<bool> Authorize(IApiConfiguration api, IApiOperation operation)
		{
			var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(api.Component);
			var e = new AuthorizationArgs(MiddlewareDescriptor.Current.UserToken, Claims.Invoke, api.Component.ToString(), "Api");

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			var r = Tenant.GetService<IAuthorizationService>().Authorize(this, e);

			if (!r.Success)
			{
				if (e.User != Guid.Empty)
					await RenderError((int)HttpStatusCode.Forbidden, SR.StatusForbiddenMessage);
				else
					await RenderError((int)HttpStatusCode.Unauthorized, SR.StatusForbiddenMessage);

				return false;
			}

			e = new AuthorizationArgs(MiddlewareDescriptor.Current.UserToken, Claims.Invoke, operation.Id.ToString(), "Api operation");

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			r = Tenant.GetService<IAuthorizationService>().Authorize(this, e);

			if (!r.Success)
			{
				if (r.Reason == AuthorizationResultReason.Empty)
					return true;

				if (e.User != Guid.Empty)
					await RenderError((int)HttpStatusCode.Forbidden, SR.StatusForbiddenMessage);
				else
					await RenderError((int)HttpStatusCode.Unauthorized, SR.StatusForbiddenMessage);

				return false;
			}

			return true;
		}

		private JObject ParseArguments()
		{
			if (Shell.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
				return Formatter.ParseArguments();
			else
			{
				var r = new JObject();

				foreach (var i in Shell.HttpContext.Request.Query.Keys)
					r.Add(i, Shell.HttpContext.Request.Query[i].ToString());

				return r;
			}
		}

		private ApiFormatter Formatter
		{
			get
			{
				if (_formatter == null)
				{
					var contentType = Shell.HttpContext.Request.ContentType;

					if (contentType.Contains(';'))
						contentType = contentType.Split(';')[0].Trim();

					if (string.Compare(contentType, JsonApiFormatter.ContentType, true) == 0)
						_formatter = new JsonApiFormatter();
					else if (string.Compare(contentType, FormApiFormatter.ContentType, true) == 0)
						_formatter = new FormApiFormatter();
					else
						throw new BadRequestException($"{SR.ErrContentTypeNotSupported} ({contentType})");

					_formatter.Context = Shell.HttpContext;
				}

				return _formatter;
			}
		}

		private async Task RenderError(int statusCode, string content)
		{
			await Formatter.RenderError(statusCode, content);
		}

		private async Task RenderResult(object content)
		{
			await Formatter.RenderResult(content);
		}

		private async Task<IApiConfiguration> GetConfiguration()
		{
			MicroService = Tenant.GetService<IMicroServiceService>().Select(MicroServiceName);

			if (MicroService == null)
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, MicroServiceName));

			var component = Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, "Api", Api);

			if (component == null)
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrComponentNotFound, Api));
				return null;
			}

			Api = component.Name;

			Initialize(Endpoint);

			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IApiConfiguration config))
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrCannotFindConfiguration, Api));
				return null;
			}

			if (!config.Protocols.Rest)
			{
				await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1})", SR.ErrApiProtocolRestDisabled, Api));
				return null;
			}

			if (config.Scope != ElementScope.Public)
			{
				await RenderError((int)HttpStatusCode.MethodNotAllowed, SR.ErrScopeError);
				return null;
			}

			return config;
		}

		private async Task<IApiOperation> GetOperation(IApiConfiguration config)
		{
			var routeData = Shell.HttpContext.GetRouteData();
			var operation = routeData.Values["operation"].ToString();

			var op = config.Operations.FirstOrDefault(f => string.Equals(f.Name, operation, System.StringComparison.OrdinalIgnoreCase));

			if (op == null)
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrServiceOperationNotFound, Api, Operation));
				return null;
			}

			Operation = op.Name;

			if (!op.Protocols.Rest)
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrApiOperationProtocolRestDisabled, Api, Operation));
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
							await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Post:
						if (!Shell.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}
						break;
					case ApiOperationVerbs.Patch:
						if (!Shell.HttpContext.Request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Delete:
						if (!Shell.HttpContext.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Put:
						if (!Shell.HttpContext.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
					case ApiOperationVerbs.Head:
						if (!Shell.HttpContext.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
						{
							SetAllowHeader(op.Protocols.RestVerbs);
							await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
							return null;
						}

						break;
				}
			}

			if (op.Scope != ElementScope.Public)
			{
				await RenderError((int)HttpStatusCode.NotFound, SR.ErrScopeError);
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