using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class GlyphAttribute : Attribute
	{
		public GlyphAttribute(string glyph)
		{
			Glyph = glyph;
		}
		public string Glyph { get; }
	}
}
