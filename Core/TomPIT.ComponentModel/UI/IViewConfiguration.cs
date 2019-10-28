using TomPIT.ComponentModel.Diagnostics;

namespace TomPIT.ComponentModel.UI
{
	public interface IViewConfiguration : IConfiguration, IGraphicInterface
	{
		//IServerEvent Invoke { get; }

		string Url { get; }
		string Layout { get; }
		bool Enabled { get; }
		IMetricOptions Metrics { get; }
	}
}
