namespace TomPIT.ComponentModel.UI
{
	public interface IView : IConfiguration, IGraphicInterface
	{
		string Url { get; }
		string Layout { get; }

		ListItems<ISnippet> Snippets { get; }
		IMetricConfiguration Metrics { get; }
	}
}
