namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTSchema : IConfiguration
	{
		ListItems<IIoTSchemaField> Fields { get; }
		ListItems<IIoTTransaction> Transactions { get; }
	}
}
