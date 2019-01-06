namespace TomPIT.ComponentModel.UI
{
	public interface IPartialView : IConfiguration, IGraphicInterface
	{
		ListItems<IViewHelper> Helpers { get; }
	}
}
