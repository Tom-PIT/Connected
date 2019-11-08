using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property| AttributeTargets.Class)]
	public class EventArgumentsAttribute : Attribute
	{
		public EventArgumentsAttribute() { }

		public EventArgumentsAttribute(string typeName)
		{
			TypeName = typeName;
		}

		public EventArgumentsAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }

	}
}
