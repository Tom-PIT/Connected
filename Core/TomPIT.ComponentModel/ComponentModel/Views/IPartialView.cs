namespace TomPIT.ComponentModel.Views
{
	public interface IPartialView : IConfiguration, IGraphicInterface
	{
		ListItems<IViewHelper> Helpers { get; }
	}
}
