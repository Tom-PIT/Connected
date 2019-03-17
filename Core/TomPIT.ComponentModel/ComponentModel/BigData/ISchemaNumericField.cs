namespace TomPIT.ComponentModel.BigData
{
	public enum NumericType
	{
		Byte = 1,
		Short = 2,
		Int = 3,
		Long = 4,
		Float = 5
	}

	public enum NumericAggregateMode
	{
		None = 1,
		Sum = 2
	}

	public interface ISchemaNumericField : ISchemaField
	{
		NumericType Type { get; }
		NumericAggregateMode Aggregate { get; }
	}
}
