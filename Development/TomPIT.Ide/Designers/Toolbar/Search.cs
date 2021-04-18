using TomPIT.Design.Ide;

namespace TomPIT.Ide.Designers.Toolbar
{
	public class Search : ToolbarAction
	{
		public Search(IEnvironment environment) : base(environment)
		{
			View = "~/Views/Ide/Actions/Search.cshtml";
			Group = "search";
			Id = "search";
		}
	}
}
