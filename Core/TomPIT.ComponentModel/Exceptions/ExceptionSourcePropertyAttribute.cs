using System;

namespace TomPIT.Exceptions
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
