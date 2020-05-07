using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
using TomPIT.Converters;

namespace TomPIT
{
	public static class Types
	{
		private static readonly string[] FileSizes = { "B", "KB", "MB", "GB", "TB" };

		public static bool Compare(object left, object right)
		{
			if (left == null && right == null)
				return true;

			if (left == null && right != null)
				return false;

			if (left != null && right == null)
				return false;

			if (!TryConvert(left, out string leftString))
				return false;

			if (!TryConvert(right, out string rightString))
				return false;

			if (Guid.TryParse(leftString, out Guid lg) && Guid.TryParse(rightString, out Guid rg))
				return lg == rg;

			return string.Compare(leftString, rightString, true) == 0;
		}

		public static bool TryConvertInvariant<T>(object value, out T result)
		{
			return TypeConverter.TryConvertInvariant<T>(value, out result);
		}

		public static bool TryConvertInvariant(object value, out object result, Type destinationType)
		{
			return TypeConverter.TryConvert(value, destinationType, out result, CultureInfo.InvariantCulture);
		}

		public static bool TryConvert(object value, out object result, Type destinationType)
		{
			return TypeConverter.TryConvert(value, destinationType, out result);
		}

		public static bool TryConvert<T>(object value, out T result)
		{
			return TypeConverter.TryConvertTo<T>(value, out result);
		}

		public static bool TryConvert<T>(object value, out T result, CultureInfo culture)
		{
			return TypeConverter.TryConvertTo<T>(value, out result, culture);
		}

		public static T Convert<T>(object value)
		{
			return TypeConverter.ConvertTo<T>(value);
		}

		public static object Convert(object value, Type destinationType)
		{
			return TypeConverter.Convert(value, destinationType);
		}

		public static T Convert<T>(object value, CultureInfo culture)
		{
			return TypeConverter.ConvertTo<T>(value, culture);
		}

		public static Type ToType(DataType dataType)
		{
			switch (dataType)
			{
				case DataType.String:
					return typeof(string);
				case DataType.Integer:
					return typeof(int);
				case DataType.Float:
					return typeof(double);
				case DataType.Date:
					return typeof(DateTime);
				case DataType.Bool:
					return typeof(bool);
				case DataType.Guid:
					return typeof(Guid);
				case DataType.Binary:
					return typeof(byte[]);
				case DataType.Long:
					return typeof(long);
				default:
					return typeof(string);
			}
		}

		public static DataType ToDataType(DbType type)
		{
			switch (type)
			{
				case DbType.AnsiString:
				case DbType.AnsiStringFixedLength:
				case DbType.String:
				case DbType.StringFixedLength:
				case DbType.Xml:
					return DataType.String;
				case DbType.Binary:
				case DbType.Object:
					return DataType.Binary;
				case DbType.Boolean:
					return DataType.Bool;
				case DbType.Byte:
				case DbType.Int16:
				case DbType.Int32:
				case DbType.SByte:
				case DbType.UInt16:
				case DbType.UInt32:
					return DataType.Integer;
				case DbType.Int64:
				case DbType.UInt64:
					return DataType.Long;
				case DbType.Currency:
				case DbType.Decimal:
				case DbType.Double:
				case DbType.Single:
				case DbType.VarNumeric:
					return DataType.Float;
				case DbType.Date:
				case DbType.DateTime:
				case DbType.DateTime2:
				case DbType.DateTimeOffset:
				case DbType.Time:
					return DataType.Date;
				case DbType.Guid:
					return DataType.Guid;
				default:
					throw new NotSupportedException();
			}
		}

		public static DbType ToDbType(DataType type)
		{
			return type switch
			{
				DataType.String => DbType.String,
				DataType.Integer => DbType.Int32,
				DataType.Float => DbType.Single,
				DataType.Date => DbType.DateTime2,
				DataType.Bool => DbType.Boolean,
				DataType.Guid => DbType.Guid,
				DataType.Binary => DbType.Binary,
				DataType.Long => DbType.Int64,
				_ => DbType.String
			};
		}

		public static DataType ToDataType(Type type)
		{
			if (type == typeof(byte)
				|| type == typeof(sbyte)
				|| type == typeof(short)
				|| type == typeof(ushort)
				|| type == typeof(int)
				|| type == typeof(uint))
				return DataType.Integer;
			else if (type == typeof(char)
				|| type == typeof(string))
				return DataType.String;
			else if (type == typeof(Guid))
				return DataType.Guid;
			else if (type == typeof(bool))
				return DataType.Bool;
			else if (type == typeof(byte[]))
				return DataType.Binary;
			else if (type == typeof(DateTime)
				|| type == typeof(TimeSpan)
				|| type == typeof(DateTimeOffset))
				return DataType.Date;
			else if (type == typeof(float)
				|| type == typeof(double)
				|| type == typeof(decimal))
				return DataType.Float;
			else if (type == typeof(long)
				|| type == typeof(ulong))
				return DataType.Long;
			else
				return DataType.String;
		}

		/// <summary>
		/// This method verifies if specified value has a valid value. It checks for empty strings,
		/// dates, guids and DBNull.Value.
		/// </summary>
		/// <param name="value">The vaue to check.</param>
		/// <returns>true if the value is considered empty, false otherwise.</returns>
		public static bool IsValueDefined(object value)
		{
			if (value == null || value == DBNull.Value)
				return true;

			if (value is string)
				return !string.IsNullOrWhiteSpace(value.ToString());
			else if (value is DateTime)
				return ((DateTime)value) != DateTime.MinValue;
			else if (value is Guid)
				return ((Guid)value) != Guid.Empty;

			return false;
		}

		public static string ToBase16(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			var sb = new StringBuilder();

			foreach (char i in value)
			{
				var v = System.Convert.ToInt32(i);

				sb.Append(v.ToString("X"));
			}

			return sb.ToString();
		}

		public static string FromBase16(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			var sb = new StringBuilder();

			for (int i = 0; i < value.Length; i += 2)
			{
				var v = System.Convert.ToInt32(string.Concat(value[i], value[i + 1]), 16);
				sb.Append(char.ConvertFromUtf32(v));
			}

			return sb.ToString();
		}

		public static string ToBase64(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		}

		public static string FromBase64(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			return Encoding.UTF8.GetString(System.Convert.FromBase64String(value));
		}

		public static string ToFileSize(double value, string measureUnitCss)
		{
			var order = 0;

			while (value >= 1024 && order < FileSizes.Length - 1)
			{
				order++;
				value /= 1024;
			}

			if (string.IsNullOrWhiteSpace(measureUnitCss))
				return string.Format("{0:0.##} {1}", value, FileSizes[order]);
			else
				return string.Format("{0:0.##} <span class=\"{2}\">{1}</span>", value, FileSizes[order], measureUnitCss);
		}

		public static string ToFileSize(double value)
		{
			return ToFileSize(value, string.Empty);
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

		public static string ToCurrentCulture(object value)
		{
			if (value == null || value == DBNull.Value)
				return string.Empty;

			var converter = System.ComponentModel.TypeDescriptor.GetConverter(value);

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

		public static string TextWithNumber(string text, int number)
		{
			if (number == 0)
				return text;
			else
				return string.Format("{0} ({1})", text, number.ToString("n0"));
		}

		public static string TextWithNumber(string text, ICollection items)
		{
			if (items == null || items.Count == 0)
				return text;

			return TextWithNumber(text, items.Count);
		}

		public static DateTime FromUtc(DateTime value, TimeZoneInfo timeZone)
		{
			if (value == DateTime.MinValue)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), timeZone);
		}

		public static DateTime ToUtc(DateTime value, TimeZoneInfo timeZone)
		{
			if (value == DateTime.MinValue)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), timeZone);
		}
	}
}
