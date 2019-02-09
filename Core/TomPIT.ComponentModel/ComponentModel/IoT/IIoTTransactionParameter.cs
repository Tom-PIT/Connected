namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTTransactionParameter : IConfigurationElement
	{
		string Name { get; }
		DataType DataType { get; }
		bool IsNullable { get; }
	}
}
