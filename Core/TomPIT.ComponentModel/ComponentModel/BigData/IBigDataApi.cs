using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.BigData
{
	public enum TimestampBehavior
	{
		Static = 1,
		Dynamic = 2
	}

	public interface IBigDataApi : IConfiguration
	{
		IServerEvent Invoke { get; }
		IServerEvent Complete { get; }

		string Key { get; }
		TimestampBehavior Timestamp { get; }

		ListItems<ISchemaField> Schema { get; }
	}
}
