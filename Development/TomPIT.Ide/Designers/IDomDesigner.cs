using Newtonsoft.Json.Linq;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;
using TomPIT.Ide.Resources;

namespace TomPIT.Ide.Designers
{
	public interface IDomDesigner : IEnvironmentObject, IDomObject
	{
		IDesignerToolbar Toolbar { get; }
		IDesignerActionResult Action(JObject data);
		IToolbox Toolbox { get; }
		string View { get; }
		string PropertyView { get; }
		object ViewModel { get; }
		string Path { get; }
		bool SupportsChaining { get; }

		bool IsPropertyEditable(string propertyName);
	}
}
