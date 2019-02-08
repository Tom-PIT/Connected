using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.UI;

namespace TomPIT.IoT.UI
{
	public class Theme : ComponentConfiguration, ITheme
	{
		private ListItems<IThemeFile> _stylesheets = null;

		public const string ComponentCategory = "Theme";

		[Items("TomPIT.IoT.Design.Items.StylesheetCollection, TomPIT.IoT.Design")]
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
