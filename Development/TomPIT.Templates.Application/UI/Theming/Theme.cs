using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI.Theming
{
	public class Theme : ComponentConfiguration, IThemeConfiguration
	{
		private ListItems<IThemeFile> _stylesheets = null;

		public const string ComponentCategory = "Theme";

		[Items(DesignUtils.StylesheetItems)]
		public ListItems<IThemeFile> Stylesheets
		{
			get
			{
				if (_stylesheets == null)
					_stylesheets = new ListItems<IThemeFile> { Parent = this };

				return _stylesheets;
			}
		}
	}
}
