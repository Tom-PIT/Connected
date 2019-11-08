using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property)]
	public class TagEditorAttribute : Attribute
	{
		public bool AllowCustomValues { get; set; }
		public bool SelectionControls { get; set; }
	}
}
