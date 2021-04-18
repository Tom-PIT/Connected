namespace TomPIT.Navigation
{
	public interface ISiteMapElement
	{
		ISiteMapElement Parent { get; }
		string Text { get; }
		bool Visible { get; }
		string Category { get; }
		string Glyph { get; }
		string Css { get; }
		int Ordinal { get; }
	}
}
