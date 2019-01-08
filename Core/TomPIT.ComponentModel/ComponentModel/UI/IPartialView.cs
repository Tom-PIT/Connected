namespace TomPIT.ComponentModel.UI
{
	public interface IPartialView : IConfiguration, IGraphicInterface
	{
		ListItems<ISnippet> Snippets { get; }
	}
}
