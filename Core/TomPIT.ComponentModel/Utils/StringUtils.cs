using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using TomPIT.ComponentModel;
using TomPIT.Globalization;
using TomPIT.Runtime;

namespace TomPIT
{
	public enum CurrencyType
	{
		Euro = 1,
	}


	public static class StringUtils
	{
		public const char TagSeparator = ',';

		public static string EllipseString(string value, int maxLength)
		{
			if (maxLength < 1)
				return value;

			if (string.IsNullOrWhiteSpace(value))
				return value;

			if (value.Length > maxLength)
				return string.Format("{0}...", value.Substring(0, maxLength - 3));
			else
				return value;
		}

		public static string Currency(double value, CurrencyType type, CultureInfo culture)
		{
			return Currency(value, type, culture, 2);
		}

		public static string Currency(double value, CurrencyType type, CultureInfo culture, int decimalPlaces)
		{
			switch (type)
			{
			case CurrencyType.Euro:
				return string.Format("{0} €", value.ToString(string.Concat("n", decimalPlaces)));
			default:
				return string.Format(value.ToString(string.Concat("n", decimalPlaces)));
			}
		}

		public static string Currency(double value, CurrencyType type)
		{
			return Currency(value, type, Thread.CurrentThread.CurrentCulture);
		}

		public static string Currency(double value, CurrencyType type, int decimalPlaces)
		{
			return Currency(value, type, Thread.CurrentThread.CurrentCulture, decimalPlaces);
		}


		public static string TextWithNumber(string text, int number)
		{
			if (number == 0)
				return text;
			else
				return string.Format("{0} ({1})", text, number.ToString("n0"));
		}

		public static string TextWithNumber(string text, ICollection items)
		{
			if (items == null)
				return text;

			return TextWithNumber(text, items.Count);
		}

		public static string ResolveLocalizationString(string value, CultureInfo culture)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			var lines = value.Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var sb = new StringBuilder();

			foreach (var i in lines)
			{
				if (sb.Length > 0)
					sb.AppendLine();

				string[] tokens = i.Split(' ');

				foreach (string s in tokens)
				{
					if (s.StartsWith("s:"))
						sb.AppendFormat("{0} ", SR.ResourceManager.GetString(s.Substring(2), culture));
					else
						sb.AppendFormat("{0} ", s);
				}

			}
			return HttpUtility.HtmlDecode(sb.ToString().Trim());
		}

		public static string ResolveLocalizationString(string value)
		{
			return ResolveLocalizationString(value, CultureInfo.CurrentCulture);
		}

		public static string StripHtml(string value)
		{
			string result = System.Text.RegularExpressions.Regex.Replace(value, "<[^>]+?>", "");

			result = System.Text.RegularExpressions.Regex.Replace(result, "&nbsp;", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result, @"&bull;", " * ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result, @"&lsaquo;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result, @"&rsaquo;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result, @"&frasl;", "/", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result, @"&lt;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			result = System.Text.RegularExpressions.Regex.Replace(result, @"&gt;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			return result;
		}

		public static string ToCurrentCulture(object value)
		{
			if (value == null || value == DBNull.Value)
				return string.Empty;

			var converter = TypeDescriptor.GetConverter(value);

			if (converter == null)
				return value.ToString();

			return converter.ConvertToString(value);
		}

		public static string ToDisplayFormat(object value, string displayFormatString)
		{
			if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
				return string.Empty;

			if (string.IsNullOrWhiteSpace(displayFormatString))
				return ToCurrentCulture(value);

			try
			{
				return string.Format(displayFormatString, value);
			}
			catch
			{
				return ToCurrentCulture(value);
			}
		}

		public static string ResolveLocalizationString(IApplicationContext context, IElement element, string property, string defaultValue)
		{
			ILanguage l = Shell.GetService<ILanguageService>().Select(context.Services.Localization.Language);

			if (l == null)
				return defaultValue;

			string r = Shell.GetService<IMicroServiceService>().SelectString(context.Identity.AuthorityId.AsGuid(), context.Services.Localization.Language, element.Id, property);

			if (string.IsNullOrEmpty(r))
				return defaultValue;

			return r;
		}

		public static string InsertSpaces(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			var r = new StringBuilder(value.Length * 2);

			r.Append(value[0]);

			for (int i = 1; i < value.Length; i++)
			{
				if (char.IsUpper(value[i]) && value[i - 1] != ' ' && !char.IsUpper(value[i - 1]))
					r.Append(' ');

				r.Append(value[i]);
			}

			return r.ToString();
		}
	}
}
