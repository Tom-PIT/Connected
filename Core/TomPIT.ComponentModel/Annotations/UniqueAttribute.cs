using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.Annotations
{
	public class UniqueAttribute : ValidationAttribute
	{
		public override bool RequiresValidationContext => true;

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (!(validationContext.GetService(typeof(IUniqueValueProvider)) is IUniqueValueProvider provider))
				return ValidationResult.Success;

			if (!(validationContext.GetService(typeof(IDataModelContext)) is IDataModelContext model))
				return ValidationResult.Success;

			var values = provider.ProvideUniqueValues(model, validationContext.MemberName);

			if (values == null || values.Count == 0)
				return ValidationResult.Success;

			foreach(var val in values)
			{
				if (Types.Compare(val, value))
					return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
			}

			return ValidationResult.Success;
		}

		public override string FormatErrorMessage(string name)
		{
			return string.Format(SR.ValUnique, name);
		}
		public override bool IsValid(object value)
		{
			return true;
		}
	}
}
