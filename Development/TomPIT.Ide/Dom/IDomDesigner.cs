using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public interface IDomDesigner : IEnvironmentClient, IDomClient
	{
		IDesignerToolbar Toolbar { get; }
		IDesignerActionResult Action(JObject data);

		string View { get; }
		string PropertyView { get; }
		object ViewModel { get; }
		string Path { get; }
		bool SupportsChaining { get; }

		bool IsPropertyEditable(string propertyName);
	}
}
