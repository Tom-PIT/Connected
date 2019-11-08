using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers.Toolbar
{
	public class Undo : ToolbarAction
	{
		public const string ActionId = "actionUndo";

		public Undo(IEnvironment environment) : base(environment)
		{
			Glyph = "fas fa-undo";
			Id = ActionId;
			Group = "Default";
		}
	}
}
