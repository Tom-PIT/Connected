namespace TomPIT.ComponentModel.Workers
{
	public interface IWorker : IConfiguration
	{
		IMetricConfiguration Metrics { get; }
	}
}
