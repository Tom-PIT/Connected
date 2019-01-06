using System;

namespace TomPIT.Converters
{
	public class GenericStringSplitter : IStringSplitter
	{
		private readonly string _separator;

		public GenericStringSplitter()
			: this(TypeConverter.DefaultStringSeparator)
		{
		}

		public GenericStringSplitter(string seperator)
		{
			_separator = seperator ?? throw new ArgumentNullException("separator");
		}

		public string[] Split(string valueList)
		{
			return valueList.Split(new[] { _separator }, StringSplitOptions.None);
		}
	}
}