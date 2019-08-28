using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TomPIT.Data;
using TomPIT.Environment;
using TomPIT.Routing;
using TomPIT.Security;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.Services.Context
{
	internal class ContextRoutingService : ContextClient, IContextRoutingService
	{
		public ContextRoutingService(IExecutionContext context) : base(context)
		{
		}

		public string Absolute(string url)
		{
			if (!IsRelative(url))
				return url;

			try
			{
				return Context.MapPath(url);
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
			var r = Context.Connection().GetService<IInstanceEndpointService>().Url(type, verbs);

			if (string.IsNullOrWhiteSpace(r))
				throw new RuntimeException(string.Format("{0} ({1}, {2})", SR.ErrNoServer, type, verbs));

			return r;
		}

		public string Resource(IUrlHelper helper, Guid blob)
		{
			var b = Context.Connection().GetService<IStorageService>().Select(blob);

			if (b == null)
				return null;

			return helper.RouteUrl("sys.resource", new
			{
				blob,
				version = b.Version
			});
		}

		public string Avatar(Guid user)
		{
			if (user == Guid.Empty)
				return null;

			var u = Context.Connection().GetService<IUserService>().Select(user.ToString());

			if (u == null)
				return null;

			if(u.Avatar == Guid.Empty)
			{
				var image = Context.Connection().GetService<IGraphicsService>().CreateImage(u.DisplayName(), 512, 512);

				Context.Connection().GetService<IUserService>().ChangeAvatar(u.Token, image, "Image/png", $"{u.DisplayName()}.png");

				u = Context.Connection().GetService<IUserService>().Select(user.ToString());
			}

			var blob = Context.Connection().GetService<IStorageService>().Select(u.Avatar);

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

		public string GenerateUrl(string primaryKey, string text, Dictionary<string,string> existing)
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

		public string ParseUrl(string template, IDictionary<string,object> parameters)
		{
			var tokens = template.Split('/');
			var compiledTokens = new StringBuilder();

			foreach(var token in tokens)
			{
				if (token.StartsWith('{') && token.EndsWith('}') && parameters !=null)
				{
					var key = token.Substring(1, token.Length - 2);
					var match = false;

					foreach(var parameter in parameters.Keys)
					{
						if(string.Compare(parameter, key, true) == 0)
						{
							compiledTokens.Append($"{Types.Convert<string>(parameters[parameter])}/");
							match = true;
							break;
						}
					}

					if (!match)
						compiledTokens.Append($"{token}/");
				}
				else
					compiledTokens.Append($"{token}/");
			}

			return $"{Shell.HttpContext.Request.RootUrl()}/{compiledTokens.ToString().TrimEnd('/')}";
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
	}
}
