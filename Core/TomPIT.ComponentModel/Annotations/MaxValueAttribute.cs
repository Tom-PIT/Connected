using System.ComponentModel.DataAnnotations;

namespace TomPIT.Annotations
{
	public class MaxValueAttribute : RequiredAttribute
	{
		public double Value { get; }

		public MaxValueAttribute(double value)
		{
			Value = Value;
		}

		public override string FormatErrorMessage(string name)
		{
			return FormatMessage(Value.ToString());
		}

		public static string FormatMessage(string name)
		{
			return string.Format(SR.ValMaxValue, name);
		}
	}
}