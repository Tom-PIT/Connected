using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers.Toolbar
{
	public class ToolbarAction : EnvironmentObject, IDesignerToolbarAction
	{
		public ToolbarAction(IEnvironment environment) : base(environment)
		{

		}

		public bool Enabled { get; set; } = true;
		public bool Visible { get; set; } = true;
		public string Text { get; set; }
		public string View { get; set; }
		public string Id { get; set; }
		public string Glyph { get; set; }
		public string Group { get; set; }
	}
}
