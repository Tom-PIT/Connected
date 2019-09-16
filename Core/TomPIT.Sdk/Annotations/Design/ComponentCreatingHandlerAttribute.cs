using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ComponentCreatingHandlerAttribute : Attribute
	{
		public ComponentCreatingHandlerAttribute() { }

		public ComponentCreatingHandlerAttribute(string type)
		{
			TypeName = type;
		}
		public ComponentCreatingHandlerAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}