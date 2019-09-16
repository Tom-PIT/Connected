using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ExceptionSourcePropertyAttribute : Attribute
	{
		public ExceptionSourcePropertyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}
