namespace TomPIT.ComponentModel.UI
{
	public interface IGraphicInterface : ISourceCode
	{
		ListItems<IViewHelper> Helpers { get; }
		ListItems<IText> Scripts { get; }
	}
}
