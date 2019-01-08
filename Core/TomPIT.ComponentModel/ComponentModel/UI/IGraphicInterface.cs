namespace TomPIT.ComponentModel.UI
{
	public interface IGraphicInterface : IText
	{
		ListItems<IViewHelper> Helpers { get; }
		ListItems<IText> Scripts { get; }
	}
}
