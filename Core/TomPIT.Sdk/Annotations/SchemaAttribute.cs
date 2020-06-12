using System;

namespace TomPIT.Annotations
{
	public sealed class SchemaAttribute : Attribute
	{
		public string Name { get; set; }
		public string Schema { get; set; }
	}
}
