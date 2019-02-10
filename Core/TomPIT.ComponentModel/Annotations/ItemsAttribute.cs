using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ItemsAttribute : Attribute
	{
		public const string LayoutItems = "TomPIT.Design.Items.LayoutItems, TomPIT.Design";

		public ItemsAttribute() { }

		public ItemsAttribute(string typeName)
		{
			TypeName = typeName;
		}

		public ItemsAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}
