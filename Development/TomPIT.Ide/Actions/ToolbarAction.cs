using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Actions
{
	public class ToolbarAction : EnvironmentClient, IDesignerToolbarAction
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
