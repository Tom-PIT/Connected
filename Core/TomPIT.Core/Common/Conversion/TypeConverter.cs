using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace TomPIT.Conversion
{
	[Flags]
	public enum ConcatenationOptions
	{
		None = 0,
		IgnoreNull = 1,
		IgnoreEmpty = 2,
		Default = None
	}
	[Flags]
	public enum ConversionOptions
	{
		None = 0,
		EnhancedTypicalValues = 1,
		AllowDefaultValueIfNull = 2,
		AllowDefaultValueIfWhitespace = 4,
		Default = EnhancedTypicalValues
	}

	internal static class TypeConverter
	{
		public class EnumerableStringConversion<T> : EnumerableConversion<T>
		{
			private bool _ignoreEmptyElements;
			private bool _trimStart;
			private bool _trimEnd;
			private string[] _nullValues = new[] { DefaultNullStringValue };

			internal EnumerableStringConversion(string valueList, IStringSplitter stringSplitter)
				: base(stringSplitter.Split(valueList))
			{
			}

			internal EnumerableStringConversion(string valueList, Type destinationType, IStringSplitter stringSplitter)
				: base(stringSplitter.Split(valueList), destinationType)
			{
			}

			public EnumerableStringConversion<T> IgnoringEmptyElements()
			{
				_ignoreEmptyElements = true;

				return this;
			}

			public EnumerableStringConversion<T> TrimmingStartOfElements()
			{
				_trimStart = true;

				return this;
			}

			public EnumerableStringConversion<T> TrimmingEndOfElements()
			{
				_trimEnd = true;

				return this;
			}

			public EnumerableStringConversion<T> WithNullBeing(params string[] nullValues)
			{
				_nullValues = nullValues;

				return this;
			}

			protected override IEnumerable GetValuesToConvert()
			{
				List<string> valuesToConvert = new List<string>();
				string valueToConvert;

				foreach (string value in base.GetValuesToConvert())
				{
					valueToConvert = PreProcessValueToConvert(value);

					if (ValueShouldBeIgnored(valueToConvert))
						continue;

					valuesToConvert.Add(valueToConvert);
				}

				return valuesToConvert;
			}

			private string PreProcessValueToConvert(string value)
			{
				string valueToConvert = value;

				if (_trimStart)
					valueToConvert = valueToConvert.TrimStart();

				if (_trimEnd)
					valueToConvert = valueToConvert.TrimEnd();

				return ValueOrNull(valueToConvert);
			}

			private bool ValueShouldBeIgnored(string valueToConvert)
			{
				if (valueToConvert == String.Empty && _ignoreEmptyElements)
					return true;

				return false;
			}

			private string ValueOrNull(string value)
			{
				if (_nullValues == null)
					return value;

				string result = value;

				if (_nullValues.Contains(value))
					result = null;

				return result;
			}
		}
		public class EnumerableConversion<T> : IEnumerable<T>
		{
			private readonly IEnumerable _valuesToConvert;
			private readonly Type _destinationType = typeof(T);
			private CultureInfo mCulture;
			private ConversionOptions _conversionOptions = ConversionOptions.Default;
			private bool _ignoreNullElements;
			private bool _ignoreNonConvertibleElements;

			private CultureInfo Culture
			{
				get { return mCulture ?? DefaultCulture; }
			}

			internal EnumerableConversion(IEnumerable values, Type destinationType)
				: this(values)
			{
				_destinationType = destinationType;
			}

			internal EnumerableConversion(IEnumerable values)
			{
				_valuesToConvert = values;
			}

			public EnumerableConversion<T> UsingCulture(CultureInfo culture)
			{
				mCulture = culture;

				return this;
			}

			public EnumerableConversion<T> UsingConversionOptions(ConversionOptions options)
			{
				_conversionOptions = options;

				return this;
			}

			public EnumerableConversion<T> IgnoringNonConvertibleElements()
			{
				_ignoreNonConvertibleElements = true;

				return this;
			}

			public EnumerableConversion<T> IgnoringNullElements()
			{
				_ignoreNullElements = true;

				return this;
			}

			public bool Try(out IEnumerable<T> result)
			{
				return TryConvert(out result);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<T> GetEnumerator()
			{
				Exception exception;

				if (TryConvert(out IEnumerable<T> result, out exception))
					return result.GetEnumerator();

				throw exception;
			}

			private bool TryConvert(out IEnumerable<T> result)
			{
				Exception exception;

				return TryConvert(out result, out exception);
			}

			private bool TryConvert(out IEnumerable<T> result, out Exception exception)
			{
				List<T> convertedValues = new List<T>();

				foreach (object value in GetValuesToConvert())
				{
					if (value == null && _ignoreNullElements)
						continue;

					if (!TypeConverter.TryConvert(value, _destinationType, out object convertedValue, Culture, _conversionOptions))
					{
						if (_ignoreNonConvertibleElements)
							continue;

						result = null;
						exception = new InvalidConversionException(value, _destinationType);

						return false;
					}

					convertedValues.Add((T)convertedValue);
				}

				result = convertedValues;
				exception = null;

				return true;
			}

			protected virtual IEnumerable GetValuesToConvert()
			{
				return _valuesToConvert;
			}
		}
		private const string ImplicitOperatorMethodName = "op_Implicit";
		private const string ExplicitOperatorMethodName = "op_Explicit";
		public static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
		public const string DefaultNullStringValue = ".null.";
		public const string DefaultStringSeparator = ";";

		public static bool TryConvertInvariant<T>(object value, out T r)
		{
			try
			{
				r = ConvertTo<T>(value, CultureInfo.InvariantCulture, ConversionOptions.AllowDefaultValueIfNull | ConversionOptions.AllowDefaultValueIfWhitespace | ConversionOptions.EnhancedTypicalValues);

				return true;
			}
			catch
			{

				r = default(T);

				return false;
			}
		}

		public static bool CanConvertTo<T>(object value)
		{
			return TryConvertTo(value, out T result);
		}

		public static bool CanConvertTo<T>(object value, CultureInfo culture)
		{
			return TryConvertTo(value, out T result, culture);
		}

		public static bool CanConvertTo<T>(object value, ConversionOptions options)
		{
			return TryConvertTo(value, out T result, options);
		}

		public static bool CanConvertTo<T>(object value, CultureInfo culture, ConversionOptions options)
		{
			return TryConvertTo(value, out T result, culture, options);
		}

		public static bool TryConvertTo<T>(object value, out T result)
		{
			return TryConvertTo(value, out result, DefaultCulture);
		}

		public static bool TryConvertTo<T>(object value, out T result, CultureInfo culture)
		{
			return TryConvertTo(value, out result, culture, ConversionOptions.Default);
		}

		public static bool TryConvertTo<T>(object value, out T result, ConversionOptions options)
		{
			return TryConvertTo(value, out result, DefaultCulture, options);
		}

		public static bool TryConvertTo<T>(object value, out T result, CultureInfo culture, ConversionOptions options)
		{
			if (TryConvert(value, typeof(T), out object tmpResult, culture, options))
			{
				result = (T)tmpResult;
				return true;
			}
			result = default(T);
			return false;
		}

		public static T ConvertTo<T>(object value)
		{
			return ConvertTo<T>(value, DefaultCulture);
		}

		public static T ConvertTo<T>(object value, CultureInfo culture)
		{
			return ConvertTo<T>(value, culture, ConversionOptions.Default);
		}

		public static T ConvertTo<T>(object value, ConversionOptions options)
		{
			return ConvertTo<T>(value, DefaultCulture, options);
		}

		public static T ConvertTo<T>(object value, CultureInfo culture, ConversionOptions options)
		{
			return (T)Convert(value, typeof(T), culture, options);
		}

		public static bool CanConvert(object value, Type destinationType)
		{
			return TryConvert(value, destinationType, out object result);
		}

		public static bool CanConvert(object value, Type destinationType, CultureInfo culture)
		{
			return TryConvert(value, destinationType, out object result, culture);
		}

		public static bool CanConvert(object value, Type destinationType, ConversionOptions options)
		{
			return TryConvert(value, destinationType, out object result, options);
		}

		public static bool CanConvert(object value, Type destinationType, CultureInfo culture, ConversionOptions options)
		{
			return TryConvert(value, destinationType, out object result, culture, options);
		}

		public static bool TryConvert(object value, Type destinationType, out object result)
		{
			return TryConvert(value, destinationType, out result, DefaultCulture);
		}

		public static bool TryConvert(object value, Type destinationType, out object result, CultureInfo culture)
		{
			return TryConvert(value, destinationType, out result, culture, ConversionOptions.Default);
		}

		public static bool TryConvert(object value, Type destinationType, out object result, ConversionOptions options)
		{
			return TryConvert(value, destinationType, out result, DefaultCulture, options);
		}

		public static bool TryConvert(object value, Type destinationType, out object result, CultureInfo culture, ConversionOptions options)
		{
			if (destinationType == typeof(object))
			{
				result = value;
				return true;
			}

			if (ValueRepresentsNull(value))
				return TryConvertFromNull(destinationType, out result, options);

			if (destinationType.IsAssignableFrom(value.GetType()))
			{
				result = value;
				return true;
			}

			Type coreDestinationType = IsGenericNullable(destinationType) ? GetUnderlyingType(destinationType) : destinationType;
			object tmpResult = null;

			if (TryConvertCore(value, coreDestinationType, ref tmpResult, culture, options))
			{
				result = tmpResult;
				return true;
			}

			result = null;

			return false;
		}

		public static object Convert(object value, Type destinationType)
		{
			return Convert(value, destinationType, DefaultCulture);
		}

		public static object Convert(object value, Type destinationType, CultureInfo culture)
		{
			return Convert(value, destinationType, culture, ConversionOptions.Default);
		}

		public static object Convert(object value, Type destinationType, ConversionOptions options)
		{
			return Convert(value, destinationType, DefaultCulture, options);
		}

		public static object Convert(object value, Type destinationType, CultureInfo culture, ConversionOptions options)
		{
			if (TryConvert(value, destinationType, out object result, culture, options))
				return result;

			throw new InvalidConversionException(value, destinationType);
		}

		private static bool TryConvertSpecialValues(object value, Type destinationType, ref object result)
		{
			if (value is char && destinationType == typeof(bool))
				return TryConvertCharToBool((char)value, ref result);

			if (value is string && destinationType == typeof(bool))
				return TryConvertStringToBool((string)value, ref result);

			if (value is bool && destinationType == typeof(char))
				return ConvertBoolToChar((bool)value, out result);

			return false;
		}

		private static bool TryConvertCharToBool(char value, ref object result)
		{
			if ("1JYT".Contains(value.ToString().ToUpper()))
			{
				result = true;
				return true;
			}

			if ("0NF".Contains(value.ToString().ToUpper()))
			{
				result = false;
				return true;
			}
			return false;
		}

		private static bool TryConvertStringToBool(string value, ref object result)
		{
			List<string> trueValues = new List<string>(new[] { "1", "j", "ja", "y", "yes", "true", "t", ".t." });

			if (trueValues.Contains(value.Trim().ToLower()))
			{
				result = true;
				return true;
			}

			List<string> falseValues = new List<string>(new[] { "0", "n", "no", "nein", "false", "f", ".f." });

			if (falseValues.Contains(value.Trim().ToLower()))
			{
				result = false;
				return true;
			}

			return false;
		}

		private static bool ConvertBoolToChar(bool value, out object result)
		{
			result = value ? 'T' : 'F';
			return true;
		}

		private static bool TryConvertFromNull(Type destinationType, out object result, ConversionOptions options)
		{
			result = GetDefaultValueOfType(destinationType);

			if (result == null)
				return true;

			return (options & ConversionOptions.AllowDefaultValueIfNull) == ConversionOptions.AllowDefaultValueIfNull;
		}

		private static bool TryConvertCore(object value, Type destinationType, ref object result, CultureInfo culture, ConversionOptions options)
		{
			if (value.GetType() == destinationType)
			{
				result = value;
				return true;
			}

			if (TryConvertByDefaultTypeConverters(value, destinationType, culture, ref result))
				return true;

			if (TryConvertByIConvertibleImplementation(value, destinationType, culture, ref result))
				return true;

			if (TryConvertXPlicit(value, destinationType, ExplicitOperatorMethodName, ref result))
				return true;

			if (TryConvertXPlicit(value, destinationType, ImplicitOperatorMethodName, ref result))
				return true;

			if (TryConvertByIntermediateConversion(value, destinationType, ref result, culture, options))
				return true;

			if (destinationType.IsEnum)
			{
				if (TryConvertToEnum(value, destinationType, ref result))
					return true;
			}

			if ((options & ConversionOptions.EnhancedTypicalValues) == ConversionOptions.EnhancedTypicalValues)
			{
				if (TryConvertSpecialValues(value, destinationType, ref result))
					return true;
			}

			if ((options & ConversionOptions.AllowDefaultValueIfWhitespace) == ConversionOptions.AllowDefaultValueIfWhitespace)
			{
				if (value is string)
				{
					if (IsWhiteSpace((string)value))
					{
						result = GetDefaultValueOfType(destinationType);
						return true;
					}
				}
			}

			return false;
		}

		private static bool TryConvertByDefaultTypeConverters(object value, Type destinationType, CultureInfo culture, ref object result)
		{
			System.ComponentModel.TypeConverter converter = TypeDescriptor.GetConverter(destinationType);

			if (converter != null)
			{
				if (converter.CanConvertFrom(value.GetType()))
				{
					try
					{
						result = converter.ConvertFrom(null, culture, value);
						return true;
					}
					catch { }
				}
			}

			converter = TypeDescriptor.GetConverter(value);

			if (converter != null)
			{
				if (converter.CanConvertTo(destinationType))
				{
					try
					{
						result = converter.ConvertTo(null, culture, value, destinationType);
						return true;
					}
					catch { }
				}
			}

			return false;
		}

		private static bool TryConvertByIConvertibleImplementation(object value, Type destinationType, IFormatProvider formatProvider, ref object result)
		{
			if (value is IConvertible convertible)
			{
				try
				{
					if (destinationType == typeof(Boolean))
					{
						result = convertible.ToBoolean(formatProvider);
						return true;
					}

					if (destinationType == typeof(Byte))
					{
						result = convertible.ToByte(formatProvider);
						return true;
					}

					if (destinationType == typeof(Char))
					{
						result = convertible.ToChar(formatProvider);
						return true;
					}

					if (destinationType == typeof(DateTime))
					{
						result = convertible.ToDateTime(formatProvider);
						return true;
					}

					if (destinationType == typeof(Decimal))
					{
						result = convertible.ToDecimal(formatProvider);
						return true;
					}

					if (destinationType == typeof(Double))
					{
						result = convertible.ToDouble(formatProvider);
						return true;
					}

					if (destinationType == typeof(Int16))
					{
						result = convertible.ToInt16(formatProvider);
						return true;
					}

					if (destinationType == typeof(Int32))
					{
						result = convertible.ToInt32(formatProvider);
						return true;
					}

					if (destinationType == typeof(Int64))
					{
						result = convertible.ToInt64(formatProvider);
						return true;
					}

					if (destinationType == typeof(SByte))
					{
						result = convertible.ToSByte(formatProvider);
						return true;
					}

					if (destinationType == typeof(Single))
					{
						result = convertible.ToSingle(formatProvider);
						return true;
					}

					if (destinationType == typeof(UInt16))
					{
						result = convertible.ToUInt16(formatProvider);
						return true;
					}

					if (destinationType == typeof(UInt32))
					{
						result = convertible.ToUInt32(formatProvider);
						return true;
					}

					if (destinationType == typeof(UInt64))
					{
						result = convertible.ToUInt64(formatProvider);
						return true;
					}
				}
				catch
				{
					return false;
				}
			}

			return false;
		}

		private static bool TryConvertXPlicit(object value, Type destinationType, string operatorMethodName, ref object result)
		{
			if (TryConvertXPlicit(value, value.GetType(), destinationType, operatorMethodName, ref result))
				return true;

			if (TryConvertXPlicit(value, destinationType, destinationType, operatorMethodName, ref result))
				return true;

			return false;
		}

		private static bool TryConvertXPlicit(object value, Type invokerType, Type destinationType, string xPlicitMethodName, ref object result)
		{
			var methods = invokerType.GetMethods(BindingFlags.Public | BindingFlags.Static);

			foreach (MethodInfo method in methods.Where(m => m.Name == xPlicitMethodName))
			{
				if (destinationType.IsAssignableFrom(method.ReturnType))
				{
					var parameters = method.GetParameters();

					if (parameters.Count() == 1 && parameters[0].ParameterType == value.GetType())
					{
						try
						{
							result = method.Invoke(null, new[] { value });
							return true;
						}
						catch { }
					}
				}
			}

			return false;
		}

		private static bool TryConvertByIntermediateConversion(object value, Type destinationType, ref object result, CultureInfo culture, ConversionOptions options)
		{
			if (value is char && (destinationType == typeof(double) || destinationType == typeof(float)))
				return TryConvertCore(System.Convert.ToInt16(value), destinationType, ref result, culture, options);

			if ((value is double || value is float) && destinationType == typeof(char))
				return TryConvertCore(System.Convert.ToInt16(value), destinationType, ref result, culture, options);

			return false;
		}

		private static bool TryConvertToEnum(object value, Type destinationType, ref object result)
		{
			try
			{
				result = Enum.ToObject(destinationType, value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static EnumerableConversion<T> ConvertToEnumerable<T>(IEnumerable values)
		{
			return new EnumerableConversion<T>(values);
		}

		public static EnumerableStringConversion<T> ConvertToEnumerable<T>(string valueList)
		{
			return ConvertToEnumerable<T>(valueList, new GenericStringSplitter());
		}

		public static EnumerableStringConversion<T> ConvertToEnumerable<T>(string valueList, string seperator)
		{
			return ConvertToEnumerable<T>(valueList, new GenericStringSplitter(seperator));
		}

		public static EnumerableStringConversion<T> ConvertToEnumerable<T>(string valueList, IStringSplitter stringSplitter)
		{
			return new EnumerableStringConversion<T>(valueList, stringSplitter);
		}

		public static EnumerableConversion<object> ConvertToEnumerable(IEnumerable values, Type destinationType)
		{
			return new EnumerableConversion<object>(values, destinationType);
		}

		public static EnumerableStringConversion<object> ConvertToEnumerable(string valueList, Type destinationType)
		{
			return ConvertToEnumerable(valueList, destinationType, DefaultStringSeparator);
		}

		public static EnumerableStringConversion<object> ConvertToEnumerable(string valueList, Type destinationType, string seperator)
		{
			return ConvertToEnumerable(valueList, destinationType, new GenericStringSplitter(seperator));
		}

		public static EnumerableStringConversion<object> ConvertToEnumerable(string valueList, Type destinationType, IStringSplitter stringSplitter)
		{
			return new EnumerableStringConversion<object>(valueList, destinationType, stringSplitter);
		}

		public static string ConvertToStringRepresentation(IEnumerable values)
		{
			return ConvertToStringRepresentation(values, DefaultCulture, new GenericStringConcatenator());
		}

		public static string ConvertToStringRepresentation(IEnumerable values, string seperator)
		{
			return ConvertToStringRepresentation(values, DefaultCulture, new GenericStringConcatenator(seperator));
		}

		public static string ConvertToStringRepresentation(IEnumerable values, string seperator, string nullValue)
		{
			return ConvertToStringRepresentation(values, DefaultCulture, new GenericStringConcatenator(seperator, nullValue));
		}

		public static string ConvertToStringRepresentation(IEnumerable values, CultureInfo culture)
		{
			return ConvertToStringRepresentation(values, culture, new GenericStringConcatenator());
		}

		public static string ConvertToStringRepresentation(IEnumerable values, IStringConcatenator stringConcatenator)
		{
			return ConvertToStringRepresentation(values, DefaultCulture, stringConcatenator);
		}

		public static string ConvertToStringRepresentation(IEnumerable values, CultureInfo culture, IStringConcatenator stringConcatenator)
		{
			string[] stringValues = ConvertToEnumerable<string>(values).UsingCulture(culture).ToArray();

			return stringConcatenator.Concatenate(stringValues);
		}

		private static bool ValueRepresentsNull(object value)
		{
			return value == null || value == DBNull.Value;
		}

		private static object GetDefaultValueOfType(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}

		private static bool IsGenericNullable(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();
		}

		private static Type GetUnderlyingType(Type type)
		{
			return Nullable.GetUnderlyingType(type);
		}

		private static bool IsWhiteSpace(string value)
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (!char.IsWhiteSpace(value[i]))
					return false;
			}

			return true;
		}

		public static bool IsDefaultValue(object value)
		{
			if (value == null || value == DBNull.Value)
				return true;

			if (value is int)
				return (int)value == 0;
			else if (value is byte)
				return (byte)value == 0;
			else if (value is short)
				return (short)value == 0;
			else if (value is float)
				return (float)value == 0;
			else if (value is double)
				return (double)value == 0;
			else if (value is decimal)
				return (decimal)value == 0;
			else if (value is long)
				return (long)value == 0;
			else if (value is string)
				return string.IsNullOrWhiteSpace(value as string);
			else if (value is DateTime)
				return (DateTime)value == DateTime.MinValue;
			else if (value is Guid)
				return (Guid)value == Guid.Empty;
			else
				return false;
		}

	}
}