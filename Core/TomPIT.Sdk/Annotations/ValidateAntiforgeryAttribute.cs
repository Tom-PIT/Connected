using System;
using System.ComponentModel.DataAnnotations;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ValidateAntiforgeryAttribute : ValidationAttribute
	{
		public ValidateAntiforgeryAttribute()
		{

		}

		public ValidateAntiforgeryAttribute(bool validateRequest)
		{
			ValidateRequest = validateRequest;
		}
		public bool ValidateRequest { get; }
	}
}
