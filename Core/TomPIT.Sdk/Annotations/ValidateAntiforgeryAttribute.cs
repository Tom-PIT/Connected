using System;
using System.ComponentModel.DataAnnotations;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ValidateAntiforgeryAttribute : ValidationAttribute
	{
	}
}
