namespace TomPIT.ComponentModel.UI
{
	public interface IMasterView : IConfiguration, IGraphicInterface
	{
		ListItems<ISnippet> Snippets { get; }
	}
}
