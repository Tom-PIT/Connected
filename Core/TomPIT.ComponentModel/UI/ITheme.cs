using TomPIT.ComponentModel;

namespace TomPIT.UI
{
	public interface ITheme : IConfiguration
	{
		ListItems<IThemeFile> Stylesheets { get; }
	}
}
