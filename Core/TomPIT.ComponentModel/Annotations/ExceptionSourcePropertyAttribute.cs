using System;

namespace TomPIT.Annotations
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
