using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ValidateAntiforgeryAttribute : ValidationAttribute
	{
	}
}
