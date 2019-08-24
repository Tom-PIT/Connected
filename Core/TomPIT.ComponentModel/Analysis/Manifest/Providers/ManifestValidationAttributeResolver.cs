using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Analysis.Manifest.Entities;

namespace TomPIT.ComponentModel.Analysis.Manifest.Providers
{
	internal static class ManifestValidationAttributeResolver
	{
		public static Entities.ManifestAttribute Resolve(ValidationAttribute attribute)
		{
			var result = new Entities.ManifestAttribute();

			if (attribute is RequiredAttribute)
				result.Name = "Required";
			else if (attribute is UrlAttribute)
			{
				result.Name = "Url";
			}
			else if (attribute is StringLengthAttribute strLength)
			{
				result.Name = "StringLength";
				result.Description = $"{strLength.MinimumLength}-{strLength.MaximumLength}";
			}
			else if (attribute is MinLengthAttribute minLength)
			{
				result.Name = "MinLength";
				result.Description = $"{minLength.Length}";
			}
			else if (attribute is MaxLengthAttribute maxLength)
			{
				result.Name = "MaxLength";
				result.Description = $"{maxLength.Length}";
			}
			else if (attribute is RangeAttribute range)
			{
				result.Name = "Range";
				result.Description = $"{range.Minimum}-{range.Maximum}";
			}
			else if (attribute is MinValueAttribute minValue)
			{
				result.Name = "MinValue";
				result.Description = $"{minValue.Value}";
			}
			else if (attribute is MaxValueAttribute maxValue)
			{
				result.Name = "MaxValue";
				result.Description = $"{maxValue.Value}";
			}
			else if (attribute is ReservedValuesAttribute reserved)
			{
				result.Name = "ReservedValues";
				result.Description = $"{reserved.Values}";
			}
			else if (attribute is ValidateAntiforgeryAttribute)
			{
				result.Name = "Antiforgery";
			}
			else if (attribute is EmailAddressAttribute)
			{
				result.Name = "Email";
			}
			else if (attribute is RegularExpressionAttribute regEx)
			{
				result.Name = "RegularExpression";
				result.Description = regEx.Pattern;
			}
			else
			{ result.Name = attribute.GetType().ShortName();

				if (result.Name.EndsWith("Attribute"))
					result.Name = result.Name.Substring(result.Name.Length - 9);
			}


			return result;
		}
	}
}
