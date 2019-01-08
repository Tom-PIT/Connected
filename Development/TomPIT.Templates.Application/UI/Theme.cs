using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application.UI
{
	[Create("Theme")]
	public class Theme : ComponentConfiguration
	{
		private ListItems<ThemeFile> _stylesheets = null;

		public const string ComponentCategory = "Theme";

		[Items("TomPIT.Application.Design.Items.StylesheetCollection, TomPIT.Application.Design")]
		public ListItems<ThemeFile> Stylesheets
		{
			get
			{
				if (_stylesheets == null)
					_stylesheets = new ListItems<ThemeFile> { Parent = this };

				return _stylesheets;
			}
		}
	}
}
