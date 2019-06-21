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

			if (!provider.IsUnique(model, validationContext.MemberName))
					return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

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
