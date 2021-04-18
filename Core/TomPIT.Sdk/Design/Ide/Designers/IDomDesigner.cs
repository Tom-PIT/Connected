using Newtonsoft.Json.Linq;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Toolbox;

namespace TomPIT.Design.Ide.Designers
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
