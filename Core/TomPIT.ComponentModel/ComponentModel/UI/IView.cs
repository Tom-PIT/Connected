namespace TomPIT.ComponentModel.UI
{
	public interface IView : IConfiguration, IGraphicInterface, IMetricConfiguration
	{
		string Url { get; }
		string Layout { get; }

		ListItems<ISnippet> Snippets { get; }
	}
}
