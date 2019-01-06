using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IDesignerToolbarAction : IEnvironmentClient
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
