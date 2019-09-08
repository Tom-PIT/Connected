using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.Analysis;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Analysis.Manifest;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Search;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Navigation;
using TomPIT.Routing;
using TomPIT.Search;
using TomPIT.Security;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT
{
	public static class ComponentModelExtensions
	{
		private class Route : IRoute
		{
			private List<IRoute> _items = null;
			public string Text { get; set; }
			public string Url { get; set; }
			public bool Enabled { get; set; }
			public int Ordinal { get; set; }
			public string Glyph { get; set; }
			public string Css { get; set; }
			public bool IsActive { get; set; }
			public bool BeginGroup { get; set; }
			public string Id { get; set; }
			public List<IRoute> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IRoute>();

					return _items;
				}
			}
			public Guid Token { get; set; }
			public string Target { get; set; }
			public bool Visible { get; set; }
			public string Category { get; set; }
		}

		public static string AsDuration(this TimeSpan duration, bool format)
		{
			return AsDuration(duration, format, DurationPrecision.Millisecond);
		}

		public static string AsDuration(this TimeSpan duration, bool format, DurationPrecision precision)
		{
			var da = SR.DurationDays;
			var ha = SR.DurationHours;
			var ma = SR.DurationMinutes;
			var sa = SR.DurationSeconds;
			var msa = SR.DurationMilliseconds;


			var sb = new StringBuilder();

			if (duration.Days > 0)
			{
				sb.AppendFormat("{0}{1}", duration.Days, format ? string.Format("<span class=\"small\">{0}</span>", da) : da);

				if (duration.Hours > 0
					|| duration.Minutes > 0
					|| duration.Seconds > 0
					|| duration.Milliseconds > 0)
					sb.Append(" ");
			}

			if (precision == DurationPrecision.Day)
				return sb.ToString();

			if (duration.Hours == 0
				&& duration.Minutes == 0
				&& duration.Seconds == 0
				&& duration.Milliseconds == 0)
				return sb.ToString();

			if (duration.Hours > 0)
			{
				sb.AppendFormat("{0}{1}", duration.Hours, format ? string.Format("<span class=\"small\">{0}</span>", ha) : ha);

				if (duration.Minutes == 0
					&& duration.Seconds == 0
					&& duration.Milliseconds == 0)
					return sb.ToString();
				else
					sb.Append(" ");
			}

			if (precision == DurationPrecision.Hour)
				return sb.ToString();

			if (duration.Minutes > 0)
			{
				sb.AppendFormat("{0}{1}", duration.Minutes, format ? string.Format("<span class=\"small\">{0}</span>", ma) : ma);

				if (duration.Seconds == 0
					&& duration.Milliseconds == 0)
					return sb.ToString();
				else
					sb.Append(" ");
			}

			if (precision == DurationPrecision.Minute)
				return sb.ToString();

			if (duration.Seconds > 0)
			{
				sb.AppendFormat("{0}{1}", duration.Seconds, format ? string.Format("<span class=\"small\">{0}</span>", sa) : sa);

				if (duration.Milliseconds == 0)
					return sb.ToString();
				else
					sb.Append(" ");
			}

			if (precision == DurationPrecision.Second)
				return sb.ToString();

			if (duration.Milliseconds > 0)
				sb.AppendFormat("{0}{1}", duration.Milliseconds, format ? string.Format("<span class=\"small\">{0}</span>", msa) : msa);

			return sb.ToString();
		}

		public static string ToHtmlBreaks(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			return value.Replace(System.Environment.NewLine, " <br/>").Replace("\r\n", " <br/>").Replace("\n", " <br/>");
		}

		public static ServerUrl CreateUrl(this ISysConnection connection, string controller, string action)
		{
			return ServerUrl.Create(connection.Url, controller, action);
		}

		public static Guid Component(this IConfiguration configuration, IExecutionContext context)
		{
			var c = context.Connection().GetService<IComponentService>().SelectComponent(configuration.Component);

			return c == null ? Guid.Empty : c.Token;
		}

		public static string ComponentName(this IConfiguration configuration, ISysConnection context)
		{
			if (context == null)
				return null;

			var c = context.GetService<IComponentService>().SelectComponent(configuration.Component);

			return c == null ? string.Empty : c.Name;
		}

		public static string ComponentName(this IConfiguration configuration, IExecutionContext context)
		{
			return ComponentName(configuration, context.Connection());
		}

		public static void ValidateMicroServiceReference(this IMicroService service, ISysConnection connection, string reference)
		{
			if (string.IsNullOrWhiteSpace(reference))
				throw new RuntimeException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, "?"));

			if (string.Compare(service.Name, reference, true) == 0)
				return;

			var refs = connection.GetService<IDiscoveryService>().References(service.Name);

			if (refs == null || refs.MicroServices.FirstOrDefault(f => string.Compare(f.MicroService, reference, true) == 0) == null)
				throw new RuntimeException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, reference));
		}

		public static bool MustChangePassword(this IAuthenticationResult result)
		{
			return !result.Success
				&& (result.Reason == AuthenticationResultReason.PasswordExpired || result.Reason == AuthenticationResultReason.NoPassword);
		}

		public static DataList<T> ToDataList<T>(this JObject dataSource) where T : DataEntity
		{
			if (dataSource.IsEmpty())
				return new DataList<T>();

			var arr = dataSource.ToResults();

			return arr.ToDataList<T>();
		}

		public static DataList<T> ToDataList<T>(this JArray items) where T : DataEntity
		{
			var r = new DataList<T>();

			foreach (var i in items)
			{
				var jo = i as JObject;

				if (jo == null)
					continue;

				var instance = typeof(T).CreateInstance<T>();

				instance.Deserialize(jo);

				r.Add(instance);
			}

			return r;
		}

		public static T ToDataItem<T>(this JObject dataSource) where T : DataEntity
		{
			if (dataSource.IsEmpty())
				return default;

			var arr = dataSource.ToResults();

			return arr.ToDataItem<T>();
		}

		public static T ToDataItem<T>(this JArray items) where T : DataEntity
		{
			if (items.Count == 0)
				return default;

			var jo = items[0] as JObject;

			if (jo == null)
				return default(T);

			var instance = typeof(T).CreateInstance<T>();

			instance.Deserialize(jo);

			return instance;
		}

		public static IComponentManifest Manifest(this IComponent component, ISysConnection connection)
		{
			var config = connection.GetService<IComponentService>().SelectConfiguration(component.Token);

			if (config == null)
				return null;

			var att = config.GetType().FindAttribute<ManifestAttribute>();

			if (att == null)
				return null;

			var provider = att.Type == null ?
				Type.GetType(att.TypeName).CreateInstance<IComponentManifestProvider>()
				: att.Type.CreateInstance<IComponentManifestProvider>();

			return provider.CreateManifest(connection, component.Token);
		}

		public static T ToRequestArguments<T>(this JObject value, IDataModelContext context, bool validate = true) where T : RequestArguments
		{
			var r = typeof(T).CreateInstance<T>(new object[] { context });

			Types.Populate(Types.Serialize(value), r);

			if (validate)
				r.Validate();

			return r;
		}

		public static Dictionary<string, string> ToDictionary(this IEnumerable items, string keyProperty, string valueProperty)
		{
			var result = new Dictionary<string, string>();

			foreach (var item in items)
			{
				var keyPropertyInfo = item.GetType().GetProperty(keyProperty);
				var valuePropertyInfo = item.GetType().GetProperty(valueProperty);

				if (keyPropertyInfo == null)
					throw new RuntimeException($"SR.ErrPropertyNotFound ({keyProperty})");

				if (valuePropertyInfo == null)
					throw new RuntimeException($"SR.ErrPropertyNotFound ({valueProperty})");

				var keyValue = Types.Convert<string>(keyPropertyInfo.GetValue(item));
				var valueValue = Types.Convert<string>(valuePropertyInfo.GetValue(item));

				result.Add(keyValue, valueValue);
			}

			return result;
		}

		public static string ParseUrl(this ISiteMapRoute link, IExecutionContext context)
		{
			if (string.IsNullOrWhiteSpace(link.Template))
				return null;

			var routeData = Shell.HttpContext == null ? new RouteData() : Shell.HttpContext.GetRouteData();

			return context.Services.Routing.ParseUrl(link.Template, routeData.Values);
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IDataModelContext context, [CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey, Dictionary<string, object> parameters)
		{
			var breadcrumbs = context.Services.Routing.QueryBreadcrumbs(routeKey, parameters);

			if (breadcrumbs == null)
				return;

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IDataModelContext context, [CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey, RouteValueDictionary parameters)
		{
			var breadcrumbs = context.Services.Routing.QueryBreadcrumbs(routeKey, parameters);

			if (breadcrumbs == null)
				return;

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IDataModelContext context, [CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey)
		{
			var breadcrumbs = context.Services.Routing.QueryBreadcrumbs(routeKey);

			if (breadcrumbs == null)
				return;

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}

		}

		public static void FromSiteMap(this List<IRoute> routes, IDataModelContext context, [CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteSiteMapsProvider)]string routeKey)
		{
			var sitemap = context.Services.Routing.QuerySiteMap(routeKey);

			if (sitemap == null)
				return;

			LoadItems(context, routes, sitemap.Items);
		}

		private static void LoadItems(IDataModelContext context, List<IRoute> routes, ConnectedList<ISiteMapRoute, ISiteMapContainer> items)
		{
			foreach (var route in items)
			{
				var url = new Route
				{
					Text = route.Text,
					Url = route.ParseUrl(context)
				};

				routes.Add(url);

				LoadItems(context, url.Items, route.Routes);
			}
		}

		private static void LoadItems(IDataModelContext context, List<IRoute> routes, ConnectedList<ISiteMapRoute, ISiteMapRoute> items)
		{
			foreach (var route in items)
			{
				var url = new Route
				{
					Text = route.Text,
					Url = route.ParseUrl(context)
				};

				routes.Add(url);

				LoadItems(context, url.Items, route.Routes);
			}
		}

		public static ISiteMapContainer WithAuthorization(this ISiteMapContainer container, IExecutionContext context)
		{
			context.Connection().GetService<IAuthorizationService>().Authorize(container);

			return container;
		}

		public static void SaveUploadedFiles(this IExecutionContext context, string primaryKey)
		{
			SaveUploadedFiles(context, primaryKey, null);
		}
		public static void SaveUploadedFileAsDrafts(this IExecutionContext context, string draft)
		{
			SaveUploadedFiles(context, null, draft);
		}

		public static Guid SaveUploadedFile(this IExecutionContext context, string primaryKey, IFormFile file)
		{
			return SaveUploadedFile(context, file, primaryKey, null);
		}
		public static Guid SaveUploadedFileAsDraft(this IExecutionContext context, string draft, IFormFile file)
		{
			return SaveUploadedFile(context, file, null, draft);
		}

		private static void SaveUploadedFiles(this IExecutionContext context, string primaryKey, string draft)
		{
			if (Shell.HttpContext == null)
				return;

			var files = Shell.HttpContext.Request.Form.Files;

			if (files == null || files.Count == 0)
				return;

			foreach (var file in files)
				SaveUploadedFile(context, file, primaryKey, draft);
		}

		private static Guid SaveUploadedFile(this IExecutionContext context, IFormFile file, string primaryKey, string draft)
		{
			var ms = context.MicroService;

			var b = new Blob
			{
				ContentType = file.ContentType,
				FileName = Path.GetFileName(file.FileName),
				MicroService = ms.Token,
				Size = Convert.ToInt32(file.Length),
				ResourceGroup = ms.ResourceGroup,
				Draft = draft,
				PrimaryKey = primaryKey,
				Type = BlobTypes.UserContent
			};

			using (var s = new MemoryStream())
			{
				file.CopyTo(s);

				var buffer = new byte[file.Length];

				s.Seek(0, SeekOrigin.Begin);
				s.Read(buffer, 0, buffer.Length);

				return context.Connection().GetService<IStorageService>().Upload(b, buffer, StoragePolicy.Singleton);
			}
		}
	}
}