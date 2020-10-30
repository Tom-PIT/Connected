using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class CollectionDesignerAttribute : Attribute
	{
		public bool Sort { get; set; }
	}
}
