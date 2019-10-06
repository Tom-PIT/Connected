using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Navigation;
using TomPIT.Routing;
using TomPIT.Security;
using TomPIT.Storage;
using TomPIT.UI;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareRoutingService : MiddlewareObject, IMiddlewareRoutingService
	{
		public MiddlewareRoutingService(IMiddlewareContext context) : base(context)
		{
		}

		public string Absolute(string url)
		{
			if (!IsRelative(url))
				return url;

			try
			{
				return MapPath(url);
			}
			catch
			{
				return url;
			}
		}

		private bool IsRelative(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return false;

			try
			{
				if (Uri.TryCreate(url, UriKind.Relative, out Uri uri))
					return true;
			}
			catch { return false; }

			return false;
		}

		public string GetServer(InstanceType type, InstanceVerbs verbs)
		{
			var r = Context.Tenant.GetService<IInstanceEndpointService>().Url(type, verbs);

			if (string.IsNullOrWhiteSpace(r))
				throw new RuntimeException(string.Format("{0} ({1}, {2})", SR.ErrNoServer, type, verbs));

			return r;
		}

		public string Resource(Guid blob)
		{
			var b = Context.Tenant.GetService<IStorageService>().Select(blob);

			if (b == null)
				return null;

			return $"/sys/media/{blob}/{b.Version}";
		}

		public string Avatar(Guid user)
		{
			if (user == Guid.Empty)
				return null;

			var u = Context.Tenant.GetService<IUserService>().Select(user.ToString());

			if (u == null)
				return null;

			if (u.Avatar == Guid.Empty)
			{
				var image = Context.Tenant.GetService<IGraphicsService>().CreateImage(u.DisplayName(), 512, 512);

				Context.Tenant.GetService<IUserService>().ChangeAvatar(u.Token, image, "Image/png", $"{u.DisplayName()}.png");

				u = Context.Tenant.GetService<IUserService>().Select(user.ToString());
			}

			var blob = Context.Tenant.GetService<IStorageService>().Select(u.Avatar);

			if (blob == null)
				return null;

			return Absolute($"~/sys/avatar/{u.Avatar}/{blob.Version}");
		}

		public void NotFound()
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
		}

		public void Forbidden()
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}

		public void Redirect(string url)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
			Shell.HttpContext.Response.Redirect(url, false);
		}

		public void BadRequest()
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		}

		public string RestUrl(string route)
		{
			var appUrl = GetServer(InstanceType.Rest, InstanceVerbs.All);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoRestServer);

			return string.Format("{0}/{1}", appUrl, route);
		}

		public string IoTUrl(string route)
		{
			var appUrl = GetServer(InstanceType.IoT, InstanceVerbs.All);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoIoTServer);

			return string.Format("{0}/{1}", appUrl, route);
		}

		public string ApplicationUrl(string route)
		{
			var appUrl = GetServer(InstanceType.Application, InstanceVerbs.All);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoAppServer);

			return string.Format("{0}/{1}", appUrl, route);
		}

		public string CdnUrl(string route)
		{
			var appUrl = GetServer(InstanceType.Cdn, InstanceVerbs.All);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoAppServer);

			return string.Format("{0}/{1}", appUrl, route);
		}

		public string SearchUrl(string route)
		{
			var appUrl = GetServer(InstanceType.Search, InstanceVerbs.All);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoSearchServer);

			return string.Format("{0}/{1}", appUrl, route);
		}

		public string BigDataUrl(string route)
		{
			var appUrl = GetServer(InstanceType.BigData, InstanceVerbs.All);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoBigDataServer);

			return string.Format("{0}/{1}", appUrl, route);
		}

		[Obsolete]
		public string GenerateUrl(string primaryKey, string text, JArray existing, string displayProperty, string primaryKeyProperty)
		{
			var items = new List<IUrlRecord>();

			foreach (JObject i in existing)
			{
				var display = i.Property(displayProperty, StringComparison.OrdinalIgnoreCase);
				var pk = i.Property(primaryKeyProperty, StringComparison.OrdinalIgnoreCase);

				if (display == null || pk == null)
					continue;


				var displayValue = display.Value as JValue;
				var idValue = pk.Value as JValue;

				var txt = Types.Convert<string>(displayValue);
				var id = Types.Convert<string>(idValue);

				items.Add(new UrlRecord(id, txt));
			}

			return GenerateUrl(primaryKey, text, items);
		}

		public string GenerateUrl(string primaryKey, string text, Dictionary<string, string> existing)
		{
			var items = new List<IUrlRecord>();

			foreach (var i in existing)
				items.Add(new UrlRecord(i.Key, i.Value));

			return GenerateUrl(primaryKey, text, items);
		}

		public string GenerateUrl(string primaryKey, string text, List<IUrlRecord> existing)
		{
			return UrlGenerator.GenerateUrl(primaryKey, text, existing);
		}

		public string ParseUrl(string template)
		{
			return ParseUrl(template, null);
		}

		public string ParseUrl(string template, RouteValueDictionary parameters)
		{
			return Context.Tenant.GetService<INavigationService>().ParseUrl(template, parameters);
		}
		public string ParseUrl(string template, IDictionary<string, object> parameters)
		{
			var values = new RouteValueDictionary();

			if (parameters != null)
			{
				foreach (var parameter in parameters)
					values.Add(parameter.Key, parameter.Value);
			}

			return ParseUrl(template, values);
		}

		public string ParseRoute([CIP(CIP.RouteKeyProvider)]string routeKey)
		{
			return ParseRoute(routeKey, null);
		}

		public string ParseRoute([CIP(CIP.RouteKeyProvider)]string routeKey, RouteValueDictionary parameters)
		{
			var route = SelectRoute(routeKey);

			if (route == null)
				return null;

			return Context.Tenant.GetService<INavigationService>().ParseUrl(route.Template, parameters);
		}
		public string ParseRoute([CIP(CIP.RouteKeyProvider)]string routeKey, IDictionary<string, object> parameters)
		{
			var values = new RouteValueDictionary();

			if (parameters != null)
			{
				foreach (var parameter in parameters)
					values.Add(parameter.Key, parameter.Value);
			}

			return ParseRoute(routeKey, values);
		}

		public T RouteValue<T>(string key)
		{
			if (Shell.HttpContext == null)
				return default;

			var routeValue = Shell.HttpContext.GetRouteValue(key);

			if (routeValue == null)
				return default;

			return Types.Convert<T>(routeValue);
		}

		public ISiteMapContainer QuerySiteMap(List<string> keys)
		{
			return QuerySiteMap(keys, true);
		}
		public ISiteMapContainer QuerySiteMap(List<string> keys, bool authorize)
		{
			return Context.Tenant.GetService<INavigationService>().QuerySiteMap(keys).WithAuthorization(Context);
		}

		public ISiteMapContainer QuerySiteMap(List<string> keys, bool authorize, List<string> tags)
		{
			return Context.Tenant.GetService<INavigationService>().QuerySiteMap(keys, tags).WithAuthorization(Context);
		}

		public List<IBreadcrumb> QueryBreadcrumbs([CIP(CIP.RouteKeyProvider)]string routeKey)
		{
			return QueryBreadcrumbs(routeKey, null);
		}

		public List<IBreadcrumb> QueryBreadcrumbs([CIP(CIP.RouteKeyProvider)]string routeKey, IDictionary<string, object> parameters)
		{
			var values = new RouteValueDictionary();

			if (parameters != null)
			{
				foreach (var parameter in parameters)
					values.Add(parameter.Key, parameter.Value);
			}

			return QueryBreadcrumbs(routeKey, values);
		}

		public List<IBreadcrumb> QueryBreadcrumbs([CIP(CIP.RouteKeyProvider)]string routeKey, RouteValueDictionary parameters)
		{
			return Context.Tenant.GetService<INavigationService>().QueryBreadcrumbs(routeKey, parameters);
		}

		public ISiteMapRoute SelectRoute([CIP(CIP.RouteKeyProvider)]string routeKey)
		{
			return Context.Tenant.GetService<INavigationService>().SelectRoute(routeKey);
		}

		public string RelativePath(string path)
		{
			if (Shell.HttpContext == null)
				return null;

			if (string.IsNullOrWhiteSpace(Shell.HttpContext.Request.PathBase))
				return path;

			return path.Substring(Shell.HttpContext.Request.PathBase.Value.Length);
		}

		public string MapPath(string relativePath)
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

		public bool CompareUrls(string path1, string path2)
		{
			var p1 = MapPath(path1);
			var p2 = MapPath(path2);

			try
			{
				var left = new Uri(p1, UriKind.RelativeOrAbsolute);
				var right = new Uri(p2, UriKind.RelativeOrAbsolute);

				if (!left.IsAbsoluteUri)
					left = new Uri($"{Context.Services.Routing.RootUrl}{left}");

				if (!right.IsAbsoluteUri)
					right = new Uri($"{Context.Services.Routing.RootUrl}{right}");

				return Uri.Compare(left, right, UriComponents.HostAndPort | UriComponents.Path, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
			}
			catch
			{
				return false;
			}
		}

		public string RootUrl
		{
			get
			{
				var request = Shell.HttpContext?.Request;

				if (request == null)
					return null;

				return string.Format("{0}://{1}/{2}", request.Scheme, request.Host, request.PathBase.ToString().Trim('/')).TrimEnd('/');
			}
		}
	}
}
