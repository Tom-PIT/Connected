namespace TomPIT.Design.Ide.Designers
{
	public interface IDesignerToolbarAction : IEnvironmentObject
	{
		bool Enabled { get; }
		bool Visible { get; }
		string Text { get; }
		string View { get; }
		string Id { get; }
		string Glyph { get; }
		string Group { get; }
	}
}
