using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ItemsAttribute : Attribute
	{
		private string _typeName = null;
		private Type _type = null;

		public ItemsAttribute() { }

		public ItemsAttribute(string typeName)
		{
			TypeName = typeName;
		}

		public ItemsAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get => _typeName; set => _typeName = value; }
		public Type Type { get => _type; set => _type = value; }
	}
}
