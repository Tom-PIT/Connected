using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class RequestArgumentAttribute : Attribute
	{
		public RequestArgumentAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}
