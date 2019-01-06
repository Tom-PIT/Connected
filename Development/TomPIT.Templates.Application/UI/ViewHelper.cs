using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	[Create("Helper", nameof(Name))]
	[DomDesigner("TomPIT.Design.TemplateDesigner, TomPIT.Ide")]
	[Syntax("csharp")]
	[EventArguments(typeof(ViewHelperArguments))]
	public class ViewHelper : Text, IViewHelper
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return GetType().ShortName();

			return Name;
		}
	}
}
