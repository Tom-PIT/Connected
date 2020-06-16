using System;

namespace TomPIT.Annotations
{
	public sealed class SchemaAttribute : Attribute
	{
		public const string SchemaTypeTable = "Table";
		public const string SchemaTypeView = "View";
		public string Name { get; set; }
		public string Schema { get; set; }
		public string Type { get; set; }
		public bool Ignore { get; set; }
	}
}
