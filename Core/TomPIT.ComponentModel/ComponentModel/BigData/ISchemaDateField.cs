namespace TomPIT.ComponentModel.BigData
{
	public enum DateType
	{
		Date = 1,
		Time = 2,
		DateTime = 3
	}

	public interface ISchemaDateField : ISchemaField
	{
		DateType Type { get; }
	}
}
