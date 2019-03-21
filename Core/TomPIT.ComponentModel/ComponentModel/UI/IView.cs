namespace TomPIT.ComponentModel.UI
{
	public interface IView : IConfiguration, IGraphicInterface, ISnippetView
	{
		string Url { get; }
		string Layout { get; }

		IMetricConfiguration Metrics { get; }
	}
}
