using System;

namespace TomPIT.Converters
{
	public class InvalidConversionException : InvalidOperationException
	{
		public InvalidConversionException(object valueToConvert, Type destinationType)
			: base(String.Format("'{0}' ({1}) is not convertible to '{2}'.",
										valueToConvert,
										valueToConvert?.GetType(),
										destinationType))
		{
		}
	}
}
