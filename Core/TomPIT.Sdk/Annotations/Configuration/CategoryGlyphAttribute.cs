using System;

namespace TomPIT.Annotations.Configuration
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public sealed class CategoryGlyphAttribute : Attribute
	{
		public CategoryGlyphAttribute(string glyph)
		{
			Glyph = glyph;
		}

		public string Glyph { get; }
	}
}
