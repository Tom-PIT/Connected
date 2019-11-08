using System;
using TomPIT.Converters;

namespace TomPIT.Converters
{
	public class StringSplitter : IStringSplitter
	{
		private readonly string _separator;

		public StringSplitter()
			: this(TypeConverter.DefaultStringSeparator)
		{
		}

		public StringSplitter(string seperator)
		{
			_separator = seperator ?? throw new ArgumentNullException("separator");
		}

		public string[] Split(string valueList)
		{
			return valueList.Split(new[] { _separator }, StringSplitOptions.None);
		}
	}
}