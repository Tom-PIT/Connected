using System;
using System.ComponentModel.DataAnnotations;
using TomPIT.Reflection;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NonDefaultAttribute : ValidationAttribute
	{
		public override string FormatErrorMessage(string name)
		{
			return $"'{name}' {SR.ValNonDefault}";
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return false;

			var defaultValue = value.GetType().DefaultValue();

			return !Types.Compare(defaultValue, value);
		}
	}
}
