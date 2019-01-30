namespace TomPIT.ComponentModel
{
	public enum MetricLevel
	{
		General = 1,
		Detail = 2
	}

	public interface IMetricConfiguration
	{
		bool MetricEnabled { get; }
		MetricLevel MetricLevel { get; }
	}
}
