using TomPIT.ComponentModel;

namespace TomPIT.IoT
{
	public interface IIoTSchemaField : IElement
	{
		string Name { get; }
		DataType DataType { get; }
	}
}
