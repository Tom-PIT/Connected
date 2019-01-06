namespace TomPIT.ComponentModel.Data
{
	public interface IDataField : IElement
	{
		string Name { get; }
		DataType DataType { get; }
	}
}