using System;

namespace TomPIT.Data.DataProviders
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SchemaBrowserAttribute : Attribute
	{
		public SchemaBrowserAttribute() { }

		public SchemaBrowserAttribute(string type)
		{
			TypeName = type;
		}
		public SchemaBrowserAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}
