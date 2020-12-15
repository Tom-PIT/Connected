using System.ComponentModel.DataAnnotations;

namespace TomPIT.Annotations
{
	public class MinValueAttribute : ValidationAttribute
	{
		public double Value { get; }

		public MinValueAttribute(double value)
		{
			Value = value;
		}

		public override bool IsValid(object value)
		{
			if (!Types.TryConvert(value, out double converted))
				return false;

			return converted >= Value;
		}

		public override string FormatErrorMessage(string name)
		{
			return FormatMessage(name, Value.ToString());
		}

		public static string FormatMessage(string name, object value)
		{
			return $"{string.Format(SR.ValMinValue, value)} ({name})";
		}
	}
}