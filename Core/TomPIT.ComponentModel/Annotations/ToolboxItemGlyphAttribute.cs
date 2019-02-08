using System;

namespace TomPIT.Annotations
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
