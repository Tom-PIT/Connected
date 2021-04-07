namespace TomPIT.Ide.TextServices.Languages
{
	public enum DeltaDecorationStickiness
	{
		AlwaysGrowsWhenTypingAtEdges = 0,
		NeverGrowsWhenTypingAtEdges = 1,
		GrowsOnlyWhenTypingBefore = 2,
		GrowsOnlyWhenTypingAfter = 3
	}
	public interface IDeltaDecorationOptions
	{
		string AfterContentClassName { get; }
		string BeforeContentClassName { get; }
		string ClassName { get; }
		string FirstLineDecorationClassName { get; }
		string GlyphMarginClassName { get; }
		string GlyphMarginHoverMessage { get; }
		string HoverMessage { get; }
		string InlineClassName { get; }
		bool InlineClassNameAffectsLetterSpacing { get; }
		bool IsWholeLine { get; }
		string LinesDecorationsClassName { get; }
		string MarginClassName { get; }
		IDeltaDecorationMinimap Minimap { get; }
		int ZIndex { get; }
	}
}
