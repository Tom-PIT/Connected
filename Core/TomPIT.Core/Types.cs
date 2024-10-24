﻿using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using TomPIT.Annotations.Models;
using TomPIT.Converters;
using TomPIT.Reflection;

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

		public static Type ToType(DbType type)
		{
			switch (type)
			{
				case DbType.AnsiString:
				case DbType.AnsiStringFixedLength:
				case DbType.String:
				case DbType.StringFixedLength:
				case DbType.Xml:
					return typeof(string);
				case DbType.Binary:
				case DbType.Object:
					return typeof(object);
				case DbType.Boolean:
					return typeof(bool);
				case DbType.Byte:
					return typeof(byte);
				case DbType.Int16:
					return typeof(short);
				case DbType.Int32:
					return typeof(int);
				case DbType.SByte:
					return typeof(sbyte);
				case DbType.UInt16:
					return typeof(ushort);
				case DbType.UInt32:
					return typeof(uint);
				case DbType.Int64:
					return typeof(long);
				case DbType.UInt64:
					return typeof(ulong);
				case DbType.Currency:
				case DbType.Decimal:
					return typeof(decimal);
				case DbType.Double:
					return typeof(double);
				case DbType.Single:
					return typeof(float);
				case DbType.VarNumeric:
					return typeof(decimal);
				case DbType.Date:
				case DbType.DateTime:
				case DbType.DateTime2:
				case DbType.Time:
					return typeof(DateTime);
				case DbType.DateTimeOffset:
					return typeof(DateTimeOffset);
				case DbType.Guid:
					return typeof(Guid);
				default:
					throw new NotSupportedException();
			}
		}

		public static DbType ToDbType(PropertyInfo property)
		{
			var type = property.PropertyType;

			if (type.IsEnum)
				type = Enum.GetUnderlyingType(type);

			if (type == typeof(char) || type == typeof(string))
			{
				if (property.FindAttribute<VersionAttribute>() != null)
					return DbType.Binary;

				var str = property.FindAttribute<StringAttribute>();

				if (str == null)
					return DbType.String;

				switch (str.Kind)
				{
					case StringKind.NVarChar:
						return DbType.String;
					case StringKind.VarChar:
						return DbType.AnsiString;
					case StringKind.Char:
						return DbType.AnsiStringFixedLength;
					case StringKind.NChar:
						return DbType.StringFixedLength;
					default:
						return DbType.String;
				}
			}
			else if (type == typeof(byte))
				return DbType.Byte;
			else if (type == typeof(bool))
				return DbType.Boolean;
			else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
			{
				var att = property.FindAttribute<DateAttribute>();

				if (att == null)
					return DbType.DateTime2;

				switch (att.Kind)
				{
					case DateKind.Date:
						return DbType.Date;
					case DateKind.DateTime:
						return DbType.DateTime;
					case DateKind.DateTime2:
						return DbType.DateTime2;
					case DateKind.SmallDateTime:
						return DbType.DateTime;
					case DateKind.Time:
						return DbType.Time;
					default:
						return DbType.DateTime2;
				}
			}
			else if (type == typeof(decimal))
				return DbType.Decimal;
			else if (type == typeof(double))
				return DbType.Double;
			else if (type == typeof(Guid))
				return DbType.Guid;
			else if (type == typeof(short))
				return DbType.Int16;
			else if (type == typeof(int))
				return DbType.Int32;
			else if (type == typeof(long))
				return DbType.Int64;
			else if (type == typeof(sbyte))
				return DbType.SByte;
			else if (type == typeof(float))
				return DbType.Single;
			else if (type == typeof(TimeSpan))
				return DbType.Time;
			else if (type == typeof(ushort))
				return DbType.UInt16;
			else if (type == typeof(uint))
				return DbType.UInt32;
			else if (type == typeof(ulong))
				return DbType.UInt64;
			else if (type == typeof(byte[]))
				return DbType.Binary;
			else
				return DbType.Binary;
		}
		public static DbType ToDbType(Type type)
		{
			var underlyingType = type;

			if (underlyingType.IsEnum)
				underlyingType = Enum.GetUnderlyingType(underlyingType);

			if (underlyingType == typeof(char) || underlyingType == typeof(string))
				return DbType.String;
			else if (underlyingType == typeof(byte))
				return DbType.Byte;
			else if (underlyingType == typeof(bool))
				return DbType.Boolean;
			else if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
				return DbType.DateTime2;
			else if (underlyingType == typeof(decimal))
				return DbType.Decimal;
			else if (underlyingType == typeof(double))
				return DbType.Double;
			else if (underlyingType == typeof(Guid))
				return DbType.Guid;
			else if (underlyingType == typeof(short))
				return DbType.Int16;
			else if (underlyingType == typeof(int))
				return DbType.Int32;
			else if (underlyingType == typeof(long))
				return DbType.Int64;
			else if (underlyingType == typeof(sbyte))
				return DbType.SByte;
			else if (underlyingType == typeof(float))
				return DbType.Single;
			else if (underlyingType == typeof(TimeSpan))
				return DbType.Time;
			else if (underlyingType == typeof(ushort))
				return DbType.UInt16;
			else if (underlyingType == typeof(uint))
				return DbType.UInt32;
			else if (underlyingType == typeof(ulong))
				return DbType.UInt64;
			else if (underlyingType == typeof(byte[]))
				return DbType.Binary;
			else
				return DbType.String;
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
			if (value == DateTime.MinValue || value.Kind != DateTimeKind.Utc)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return TimeZoneInfo.ConvertTimeFromUtc(value, timeZone);
		}

		public static DateTime ToUtc(DateTime value, TimeZoneInfo timeZone)
		{
			if (value == DateTime.MinValue || value.Kind == DateTimeKind.Utc)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return TimeZoneInfo.ConvertTimeToUtc(value, timeZone);
		}

		public static DateTimeOffset FromUtc(DateTimeOffset value, TimeZoneInfo timeZone)
		{
			if (value == DateTimeOffset.MinValue)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return new DateTimeOffset(new DateTime(value.UtcDateTime.Ticks, DateTimeKind.Utc));
			else
			{
				var offset = FromUtc(value.UtcDateTime, timeZone);

				return new DateTimeOffset(offset, timeZone.GetUtcOffset(value.UtcDateTime));
			}
		}

		public static DateTimeOffset ToUtc(DateTimeOffset value, TimeZoneInfo timeZone)
		{
			if (value == DateTimeOffset.MinValue || value.Offset == TimeSpan.Zero)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return new DateTimeOffset(value.UtcDateTime);
		}

		public static DateTime ToStartOfWeek(this DateTime value, DayOfWeek? firstDayOfWeek = null)
		{
			firstDayOfWeek ??= CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
			var dayOfWeek = value.DayOfWeek;

			var difference = firstDayOfWeek.Value - dayOfWeek;

			if (difference > 0)
				difference -= 7;

			try
			{
				return value.AddDays(difference) - value.TimeOfDay;
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		public static bool IsNullableType(this Type type)
		{
			return (object)type != null && type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
	}
}
