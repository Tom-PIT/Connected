using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ViewRendererAttribute : Attribute
	{
		public ViewRendererAttribute() { }

		public ViewRendererAttribute(string typeName)
		{
			TypeName = typeName;
		}

		public ViewRendererAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}
