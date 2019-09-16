using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.Messaging;

namespace TomPIT.ComponentModel.UI
{
	public interface IViewConfiguration : IConfiguration, IGraphicInterface, ISnippetView
	{
		IServerEvent Invoke { get; }

		string Url { get; }
		string Layout { get; }
		bool Enabled { get; }
		IMetricOptions Metrics { get; }
	}
}
