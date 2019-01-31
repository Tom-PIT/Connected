namespace TomPIT.ComponentModel
{
	public enum MetricLevel
	{
		Basic = 1,
		Detail = 2
	}

	public interface IMetricConfiguration : IElement
	{
		bool Enabled { get; }
		MetricLevel Level { get; }
	}
}
