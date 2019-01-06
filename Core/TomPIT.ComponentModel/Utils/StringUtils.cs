using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;

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
