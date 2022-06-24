using TomPIT.Annotations;
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
		[CollectionDesigner(Sort = false)]
		public ListItems<IThemeFile> Stylesheets
		{
			get
			{
				if (_stylesheets == null)
					_stylesheets = new ListItems<IThemeFile> { Parent = this };

				return _stylesheets;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.ThemeItems)]
		public string BaseTheme { get; set; }
	}
}
