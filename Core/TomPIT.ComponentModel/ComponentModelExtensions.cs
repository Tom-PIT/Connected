using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT
{
	public static class ComponentModelExtensions
	{
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

		public static DataList<T> ToDataList<T>(this JObject dataSource) where T : JsonEntity
		{
			if (dataSource.IsEmpty())
				return new DataList<T>();

			var arr = dataSource.ToResults();

			return arr.ToDataList<T>();
		}

		public static DataList<T> ToDataList<T>(this JArray items) where T : JsonEntity
		{
			var r = new DataList<T>();

			foreach (var i in items)
			{
				var jo = i as JObject;

				if (jo == null)
					continue;

				var instance = typeof(T).CreateInstance<T>(new object[] { jo });

				r.Add(instance);
			}

			return r;
		}

		public static T ToDataItem<T>(this JObject dataSource) where T : JsonEntity
		{
			if (dataSource.IsEmpty())
				return default(T);

			var arr = dataSource.ToResults();

			return arr.ToDataItem<T>();
		}

		public static T ToDataItem<T>(this JArray items) where T : JsonEntity
		{
			if (items.Count == 0)
				return default(T);

			var jo = items[0] as JObject;

			if (jo == null)
				return default(T);

			var instance = typeof(T).CreateInstance<T>(new object[] { jo });

			return instance;
		}

		//public static void FromRequestArguments<T>(this List<SchemaParameter> parameters)
		//{
		//	var t = typeof(T);

		//	var properties = t.GetProperties();

		//	foreach(var i in properties)
		//	{
		//		if (i.IsPrimitive())
		//		{

		//		}
		//		else if(i.IsCollection())
		//		{

		//		}
		//		else
		//		{

		//		}

		//		if (!i.CanWrite)
		//			continue;

		//		var accessors = i.GetAccessors();
		//	}
		//}

		
	}
}
