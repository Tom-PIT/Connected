using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTSchemaConfiguration : IConfiguration
	{
		ListItems<IIoTSchemaField> Fields { get; }
		ListItems<IIoTTransaction> Transactions { get; }
	}
}
