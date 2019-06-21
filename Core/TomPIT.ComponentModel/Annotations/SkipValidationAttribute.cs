using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SkipValidationAttribute : Attribute
	{
	}
}
