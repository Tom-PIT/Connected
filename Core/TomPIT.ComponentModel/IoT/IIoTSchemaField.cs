namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTSchemaField : IElement
	{
		string Name { get; }
		DataType DataType { get; }
	}
}
