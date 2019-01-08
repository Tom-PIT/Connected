using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.UI;

namespace TomPIT.Application.UI
{
	[Create("Theme")]
	public class Theme : ComponentConfiguration, ITheme
	{
		private ListItems<IThemeFile> _stylesheets = null;

		public const string ComponentCategory = "Theme";

		[Items("TomPIT.Application.Design.Items.StylesheetCollection, TomPIT.Application.Design")]
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
