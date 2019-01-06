namespace TomPIT.ComponentModel.Data
{
	public interface IDataParameter : IElement
	{
		string Name { get; }
		DataType DataType { get; }
		bool IsNullable { get; }
		bool NullMapping { get; }
		bool SupportsTimeZone { get; }
	}
}
