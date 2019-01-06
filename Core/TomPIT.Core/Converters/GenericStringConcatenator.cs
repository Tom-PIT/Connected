using System;
using System.Linq;
using System.Text;

namespace TomPIT.Converters
{
	public class GenericStringConcatenator : IStringConcatenator
	{
		private readonly string _separator;
		private readonly string _nullValue;
		private readonly ConcatenationOptions _concatenationOptions;

		public GenericStringConcatenator()
			: this(TypeConverter.DefaultStringSeparator)
		{
		}

		public GenericStringConcatenator(string separator)
			: this(separator, ConcatenationOptions.Default)
		{
		}

		public GenericStringConcatenator(string separator, ConcatenationOptions concatenationOptions)
			: this(separator, TypeConverter.DefaultNullStringValue, concatenationOptions)
		{
		}

		public GenericStringConcatenator(string separator, string nullValue)
			: this(separator, nullValue, ConcatenationOptions.Default)
		{
		}

		public GenericStringConcatenator(string separator, string nullValue, ConcatenationOptions concatenationOptions)
		{
			if (separator == null)
				throw new ArgumentNullException("separator");

			_separator = separator;
			_nullValue = nullValue;
			_concatenationOptions = concatenationOptions;
		}

		public string Concatenate(string[] values)
		{
			string[] valuesToConcatenate = values.Where(v => !IgnoreValue(v)).Select(value => value ?? _nullValue).ToArray();

			return ConcatenateCore(valuesToConcatenate);
		}

		private bool IgnoreValue(string value)
		{
			if (value == null && (_concatenationOptions & ConcatenationOptions.IgnoreNull) == ConcatenationOptions.IgnoreNull)
				return true;

			if (value == String.Empty && (_concatenationOptions & ConcatenationOptions.IgnoreEmpty) == ConcatenationOptions.IgnoreEmpty)
				return true;

			return false;
		}

		protected virtual string ConcatenateCore(string[] values)
		{
			StringBuilder result = new StringBuilder();

			foreach (string value in values)
			{
				if (result.Length > 0)
					result.Append(_separator);

				result.Append(value);
			}

			return result.ToString();
		}
	}
}