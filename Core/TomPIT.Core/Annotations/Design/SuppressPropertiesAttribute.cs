using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class SuppressPropertiesAttribute : Attribute
	{
		public string Properties { get; private set; }

		public SuppressPropertiesAttribute() { }
		public SuppressPropertiesAttribute(string properties) { Properties = properties; }
	}
}
