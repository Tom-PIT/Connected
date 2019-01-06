using System;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Net;
using TomPIT.Runtime;
using TomPIT.Security;

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

		public static ServerUrl CreateUrl(this ISysContext context, string controller, string action)
		{
			return ServerUrl.Create(context.Url, controller, action);
		}

		public static Guid MicroService(this IApplicationContext context)
		{
			if (context.Identity == null || string.IsNullOrWhiteSpace(context.Identity.ContextId))
				return Guid.Empty;

			return context.Identity.ContextId.AsGuid();
		}

		public static Guid Component(this IConfiguration configuration, IApplicationContext context)
		{
			var c = context.GetServerContext().GetService<IComponentService>().SelectComponent(configuration.Component);

			return c == null ? Guid.Empty : c.Token;
		}

		public static string ComponentName(this IConfiguration configuration, ISysContext context)
		{
			if (context == null)
				return null;

			var c = context.GetService<IComponentService>().SelectComponent(configuration.Component);

			return c == null ? string.Empty : c.Name;
		}

		public static string ComponentName(this IConfiguration configuration, IApplicationContext context)
		{
			return ComponentName(configuration, context.GetServerContext());
		}

		public static void ValidateMicroServiceReference(this IMicroService service, ISysContext context, string reference)
		{
			if (string.IsNullOrWhiteSpace(reference))
				throw new ApiException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, "?"));

			if (string.Compare(service.Name, reference, true) == 0)
				return;

			var refs = context.GetService<IDiscoveryService>().References(service.Name);

			if (refs == null || refs.MicroServices.FirstOrDefault(f => string.Compare(f.MicroService, reference, true) == 0) == null)
				throw new ApiException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, reference));
		}

		public static bool MustChangePassword(this IAuthenticationResult result)
		{
			return !result.Success
				&& (result.Reason == AuthenticationResultReason.PasswordExpired || result.Reason == AuthenticationResultReason.NoPassword);
		}
	}
}
