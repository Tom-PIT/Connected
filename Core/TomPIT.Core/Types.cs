using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.Converters;

namespace TomPIT
{
	public static class Types
	{
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

		public static string ToFriendlyName(this Type type)
		{
			if (type == typeof(int))
				return "int";
			else if (type == typeof(short))
				return "short";
			else if (type == typeof(byte))
				return "byte";
			else if (type == typeof(bool))
				return "bool";
			else if (type == typeof(long))
				return "long";
			else if (type == typeof(float))
				return "float";
			else if (type == typeof(double))
				return "double";
			else if (type == typeof(decimal))
				return "decimal";
			else if (type == typeof(string))
				return "string";
			else if (type.IsGenericType)
				return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => ToFriendlyName(x)).ToArray()) + ">";
			else
				return type.ShortName();
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

		public static string AsString(this int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string AsString(this long value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string AsString(this float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string AsString(this double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string AsString(this short value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string AsString(this Guid value)
		{
			return value.ToString();
		}

		public static int AsInt(this string value)
		{
			return Convert<int>(value, CultureInfo.InvariantCulture);
		}

		public static long AsLong(this string value)
		{
			return Convert<long>(value, CultureInfo.InvariantCulture);
		}

		public static float AsFloat(this string value)
		{
			return Convert<float>(value, CultureInfo.InvariantCulture);
		}

		public static double AsDouble(this string value)
		{
			return Convert<double>(value, CultureInfo.InvariantCulture);
		}

		public static short AsShort(this string value)
		{
			return Convert<short>(value, CultureInfo.InvariantCulture);
		}

		public static Guid AsGuid(this string value)
		{
			return Convert<Guid>(value, CultureInfo.InvariantCulture);
		}

		public static Type GetType(string type)
		{
			var t = Type.GetType(type);

			if (t != null)
				return t;

			var asm = LoadAssembly(type);

			if (asm == null)
				return null;

			var typeName = type.Split(',')[0];

			return asm.GetType(typeName);
		}

		public static string InvariantTypeName(this Assembly assembly, string fullName)
		{
			string[] tokens = fullName.Split(',');

			if (tokens == null || tokens.Length == 0)
				return null;

			return string.Format("{0}, {1}", tokens[0].Trim(), assembly.FullName);
		}

		public static Assembly LoadAssembly(string type)
		{
			var tokens = type.Split(',');
			var libraryName = string.Empty;
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (tokens.Length == 1)
				libraryName = string.Format("{0}.dll", tokens[0].Trim());
			else if (tokens.Length > 1)
				libraryName = string.Format("{0}.dll", tokens[1].Trim());

			var file = string.Format("{0}\\{1}", path, libraryName);

			if (!File.Exists(file))
				return null;

			return AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(file));
		}

		public static string ShortName(this Assembly assembly)
		{
			return assembly.FullName.Split(',')[0];
		}

		public static object DefaultValue(this Type type)
		{
			if (type.IsValueType)
				return Activator.CreateInstance(type);

			return null;
		}

		public static bool IsNumericType(this Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static string ToFileSize(double value, string measureUnitCss)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			var order = 0;

			while (value >= 1024 && order < sizes.Length - 1)
			{
				order++;
				value = value / 1024;
			}

			if (string.IsNullOrWhiteSpace(measureUnitCss))
				return string.Format("{0:0.##} {1}", value, sizes[order]);
			else
				return string.Format("{0:0.##} <span class=\"{2}\">{1}</span>", value, sizes[order], measureUnitCss);
		}

		public static string ToFileSize(double value)
		{
			return ToFileSize(value, string.Empty);
		}
	}
}
