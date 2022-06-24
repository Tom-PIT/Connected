using TomPIT.Collections;

namespace TomPIT.ComponentModel.UI.Theming
{
	public interface IThemeConfiguration : IConfiguration
	{
		ListItems<IThemeFile> Stylesheets { get; }
		string BaseTheme { get; }
	}
}
