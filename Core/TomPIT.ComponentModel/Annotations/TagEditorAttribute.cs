using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class TagEditorAttribute : Attribute
	{
		public bool AllowCustomValues { get; set; }
		public bool SelectionControls { get; set; }
	}
}
