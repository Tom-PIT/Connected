using System;
using System.ComponentModel.DataAnnotations;
using TomPIT.Data;
using TomPIT.Middleware;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class UniqueAttribute : ValidationAttribute
	{
		public override bool RequiresValidationContext => true;

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value == null)
				return ValidationResult.Success;

			var provider = validationContext.GetService(typeof(IUniqueValueProvider)) as IUniqueValueProvider;
			var context = validationContext.GetService(typeof(IMiddlewareContext)) as IMiddlewareContext;

			if (provider == null || context == null)
				return ValidationResult.Success;

			if (!provider.IsUnique(context, validationContext.MemberName))
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
