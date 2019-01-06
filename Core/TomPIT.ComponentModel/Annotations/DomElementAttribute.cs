using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DomElementAttribute : Attribute
	{
		public DomElementAttribute() { }

		public DomElementAttribute(string type)
		{
			TypeName = type;
		}
		public DomElementAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}