using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ProxyPropertyAttribute : Attribute
	{
		public ProxyPropertyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}
