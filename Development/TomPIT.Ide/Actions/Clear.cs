using TomPIT.Ide;

namespace TomPIT.Actions
{
	public class Clear : ToolbarAction
	{
		public const string ActionId = "actionClear";

		public Clear(IEnvironment environment) : base(environment)
		{
			Glyph = "fal fa-times";
			Id = ActionId;
			Text = "Clear";
			Group = "Default";
			View = "~/Views/Ide/Actions/Clear.cshtml";
		}
	}
}
