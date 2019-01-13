using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ComponentCreateHandlerAttribute : Attribute
	{
		public ComponentCreateHandlerAttribute() { }

		public ComponentCreateHandlerAttribute(string type)
		{
			TypeName = type;
		}
		public ComponentCreateHandlerAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}