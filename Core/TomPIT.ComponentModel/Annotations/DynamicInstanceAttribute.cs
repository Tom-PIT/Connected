using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DynamicInstanceAttribute : Attribute
	{
		public DynamicInstanceAttribute() { }

		public DynamicInstanceAttribute(string type)
		{
			TypeName = type;
		}
		public DynamicInstanceAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}