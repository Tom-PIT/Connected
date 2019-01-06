using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DomDesignerAttribute : Attribute
	{
		public DomDesignerAttribute() { }

		public DomDesignerAttribute(string type)
		{
			TypeName = type;
		}
		public DomDesignerAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}