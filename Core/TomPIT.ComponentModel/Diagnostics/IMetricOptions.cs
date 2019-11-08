namespace TomPIT.ComponentModel.Diagnostics
{
	public enum MetricLevel
	{
		Basic = 1,
		Detail = 2
	}

	public interface IMetricOptions : IElement
	{
		bool Enabled { get; }
		MetricLevel Level { get; }
	}
}
