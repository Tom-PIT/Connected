using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ComponentCreatedHandlerAttribute : Attribute
	{
		public ComponentCreatedHandlerAttribute() { }

		public ComponentCreatedHandlerAttribute(string type)
		{
			TypeName = type;
		}
		public ComponentCreatedHandlerAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}