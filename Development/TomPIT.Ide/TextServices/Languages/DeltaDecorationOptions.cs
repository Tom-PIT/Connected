namespace TomPIT.Ide.TextServices.Languages
{
	internal class DeltaDecorationOptions : IDeltaDecorationOptions
	{
		public string AfterContentClassName {get;set;}
		public string BeforeContentClassName {get;set;}
		public string ClassName {get;set;}
		public string FirstLineDecorationClassName {get;set;}
		public string GlyphMarginClassName {get;set;}
		public string GlyphMarginHoverMessage {get;set;}
		public string HoverMessage {get;set;}
		public string InlineClassName {get;set;}
		public bool InlineClassNameAffectsLetterSpacing {get;set;}
		public bool IsWholeLine {get;set;}
		public string LinesDecorationsClassName {get;set;}
		public string MarginClassName {get;set;}
		public IDeltaDecorationMinimap Minimap {get;set;}
		public int ZIndex {get;set;}
	}
}
