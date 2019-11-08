using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ToolboxItemGlyphAttribute : Attribute
	{
		public ToolboxItemGlyphAttribute() { }

		public ToolboxItemGlyphAttribute(string view)
		{
			View = view;
		}

		public string View { get; }
	}
}
