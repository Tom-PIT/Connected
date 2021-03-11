using System;

namespace TomPIT.Annotations.Design.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ClassRequiredAttribute : Attribute
	{
		public string ClassNameProperty { get; set; }
	}
}
