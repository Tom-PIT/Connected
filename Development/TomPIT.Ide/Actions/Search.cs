using TomPIT.Ide;

namespace TomPIT.Actions
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
