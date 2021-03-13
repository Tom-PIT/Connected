using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

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

			if (op == null)
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrServiceOperationNotFound, Api, Operation));
				return;
			}

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

					if (string.IsNullOrWhiteSpace(contentType))
						_formatter = new JsonApiFormatter();
					else
					{
						if (contentType.Contains(';'))
							contentType = contentType.Split(';')[0].Trim();

						if (string.Compare(contentType, JsonApiFormatter.ContentType, true) == 0)
							_formatter = new JsonApiFormatter();
						else if (string.Compare(contentType, FormApiFormatter.ContentType, true) == 0)
							_formatter = new FormApiFormatter();
						else
							throw new BadRequestException($"{SR.ErrContentTypeNotSupported} ({contentType})");
					}

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
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, MicroServiceName));
				return null;
			}

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

			var op = config.Operations.FirstOrDefault(f => string.Equals(f.Name, Operation, StringComparison.OrdinalIgnoreCase));

			if (op == null)
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrServiceOperationNotFound, Api, Operation));
				return null;
			}

			Operation = op.Name;

			if (op.Scope != ElementScope.Public)
			{
				await RenderError((int)HttpStatusCode.MethodNotAllowed, SR.ErrScopeError);
				return null;
			}

			var type = Tenant.GetService<ICompilerService>().ResolveType(MicroService.Token, op, op.Name);

			if (type == null)
			{
				await RenderError(StatusCodes.Status404NotFound, string.Format("{0} ({1}/{2})", SR.ErrServiceOperationNotFound, Api, Operation));
				return null;
			}

			if (Shell.HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpGetAttribute>(type))
				return null;
			else if (Shell.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpPostAttribute>(type))
				return null;
			else if (Shell.HttpContext.Request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpPatchAttribute>(type))
				return null;
			else if (Shell.HttpContext.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpDeleteAttribute>(type))
				return null;
			else if (Shell.HttpContext.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpPutAttribute>(type))
				return null;
			else if (Shell.HttpContext.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpHeadAttribute>(type))
				return null;
			else if (Shell.HttpContext.Request.Method.Equals("TRACE", StringComparison.OrdinalIgnoreCase) && !await ValidateVerb<HttpTraceAttribute>(type))
				return null;

			return op;
		}

		private async Task<bool> ValidateVerb<T>(Type type) where T : Attribute
		{
			var att = type.FindAttribute<T>();

			if (att == null)
			{
				SetAllowHeader(type);
				await RenderError(StatusCodes.Status405MethodNotAllowed, string.Format("{0} ({1}/{2})", SR.ApiOperationInvalidVerb, Api, Operation));
				return false;
			}

			return true;
		}

		private static void SetAllowHeader(Type type)
		{
			var sb = new StringBuilder();

			if (type.FindAttribute<HttpGetAttribute>() != null)
				sb.Append($"GET,");

			if (type.FindAttribute<HttpHeadAttribute>() != null)
				sb.Append($"HEAD,");

			if (type.FindAttribute<HttpPostAttribute>() != null)
				sb.Append($"POST,");

			if (type.FindAttribute<HttpPutAttribute>() != null)
				sb.Append($"PUT,");

			if (type.FindAttribute<HttpDeleteAttribute>() != null)
				sb.Append($"DELETE,");

			if (type.FindAttribute<HttpPatchAttribute>() != null)
				sb.Append($"PATCH,");

			if (type.FindAttribute<HttpTraceAttribute>() != null)
				sb.Append($"TRACE,");

			Shell.HttpContext.Response.Headers.Add(Enum.GetName(typeof(HttpResponseHeader), HttpResponseHeader.Allow), sb.ToString().TrimEnd(','));
		}
	}
}