using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT
{
	public static class ServerExtensions
	{
		public static void Validate(this IModel model, Controller controller, JObject requestBody)
		{
			var ctx = new ValidationContext(model);

			ValidateProperties(model, ctx, requestBody);

			var results = model.Validate();

			if (results != null)
			{
				foreach (ValidationResult i in results)
				{
					if (i != null)
						controller.ModelState.AddModelError(string.Empty, i.ErrorMessage);
				}
			}
		}

		private static void ValidateProperties(this IModel model, ValidationContext context, JObject requestBody)
		{
			var props = model.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty);

			foreach (var i in props)
			{
				var attributes = i.GetCustomAttributes(false);

				foreach (var j in attributes)
				{
					var value = Types.Convert(requestBody.Optional<object>(i.Name, false), i.PropertyType);

					if (j is ValidationAttribute)
					{
						var va = j as ValidationAttribute;

						va.Validate(value, i.Name);
					}
				}
			}
		}

		public static string RouteUrl(this IExecutionContext context, string routeName, object values)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrCannotResolveHttpRequest);

			var svc = Shell.HttpContext.RequestServices.GetService(typeof(IUrlHelperFactory)) as IUrlHelperFactory;

			if (svc == null)
				throw new RuntimeException(SR.ErrNoUrlHelper);

			/*
			 * It's IRequestContextProvider for sure otherwise request would be null
			 */
			var ac = context as IRequestContextProvider;

			var helper = svc.GetUrlHelper(ac.ActionContext);

			return helper.RouteUrl(routeName, values);
		}

		public static string RelativePath(this HttpContext context, string path)
		{
			if (string.IsNullOrWhiteSpace(context.Request.PathBase))
				return path;

			return path.Substring(context.Request.PathBase.Value.Length);
		}

		public static string MapPath(this IExecutionContext context, string relativePath)
		{
			if (string.IsNullOrEmpty(relativePath))
				return null;
			else if (relativePath[0] == '~')
			{
				var segment = new PathString(relativePath.Substring(1));
				var applicationPath = Shell.HttpContext.Request.PathBase;

				return applicationPath.Add(segment).Value;
			}

			return relativePath;
		}

		public static bool CompareUrls(this IExecutionContext context, string path1, string path2)
		{
			var p1 = MapPath(context, path1);
			var p2 = MapPath(context, path2);

			try
			{
				var left = new Uri(p1, UriKind.RelativeOrAbsolute);
				var right = new Uri(p2, UriKind.RelativeOrAbsolute);

				if (!left.IsAbsoluteUri)
					left = new Uri($"{Shell.HttpContext.Request.RootUrl()}{left}");

				if (!right.IsAbsoluteUri)
					right = new Uri($"{Shell.HttpContext.Request.RootUrl()}{right}");

				return Uri.Compare(left, right, UriComponents.HostAndPort | UriComponents.Path, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
			}
			catch
			{
				return false;
			}
		}

		public static string RootUrl(this HttpRequest request)
		{
			return string.Format("{0}://{1}/{2}", request.Scheme, request.Host, request.PathBase.ToString().Trim('/')).TrimEnd('/');
		}

		public static string RootUrl(this IExecutionContext context)
		{
			var http = Shell.HttpContext?.Request;

			return string.Format("{0}://{1}/{2}", http.Scheme, http.Host, http.PathBase.ToString().Trim('/')).TrimEnd('/');
		}

		public static IConfiguration Configuration(this IElement element)
		{
			return Closest<IConfiguration>(element);
		}

		public static Guid MicroService(this IConfiguration configuration, ISysConnection connection)
		{
			var component = connection.GetService<IComponentService>().SelectComponent(configuration.Component);

			if (component == null)
				return Guid.Empty;

			return component.MicroService;
		}

		public static Guid MicroService(this IElement element, ISysConnection server)
		{
			var config = element.Configuration();

			if (config == null)
				return Guid.Empty;

			var component = server.GetService<IComponentService>().SelectComponent(config.Component);

			if (component == null)
				return Guid.Empty;

			return component.MicroService;
		}

		public static T Closest<T>(this IElement instance)
		{
			if (instance == null)
				return default(T);

			if (instance is T || instance.GetType().IsAssignableFrom(typeof(T)))
				return (T)instance;

			if (!(instance is IElement e))
				return default(T);

			return Closest<T>(e.Parent);
		}

		public static T Execute<T>(this ISysConnection connection, ISourceCode e, object sender, T args)
		{
			connection.GetService<ICompilerService>().Execute<T>(e.Configuration().MicroService(connection), e, sender, args);

			return args;
		}
	}
}
