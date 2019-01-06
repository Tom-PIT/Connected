namespace TomPIT.ComponentModel.Views
{
	public interface IMasterView : IConfiguration, IGraphicInterface
	{
		ListItems<IViewHelper> Helpers { get; }
	}
}
