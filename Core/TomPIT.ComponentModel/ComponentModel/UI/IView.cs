using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.UI
{
	public interface IView : IConfiguration, IGraphicInterface, ISnippetView
	{
		IServerEvent Invoke { get; }

		string Url { get; }
		string Layout { get; }
		bool Enabled { get; }
		IMetricConfiguration Metrics { get; }
	}
}
