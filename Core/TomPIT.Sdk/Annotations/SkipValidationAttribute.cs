using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SkipValidationAttribute : Attribute
	{
	}
}
