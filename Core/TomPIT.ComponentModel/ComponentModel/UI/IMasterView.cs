namespace TomPIT.ComponentModel.UI
{
	public interface IMasterView : IConfiguration, IGraphicInterface
	{
		ListItems<IViewHelper> Helpers { get; }
	}
}
