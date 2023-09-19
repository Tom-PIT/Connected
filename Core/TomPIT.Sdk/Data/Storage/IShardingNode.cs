namespace TomPIT.Data.Storage;

public enum Status : byte
{
    //
    // Summary:
    //     The record is enabled and can be used when adding and editing data.
    Enabled = 1,
    //
    // Summary:
    //     The record is disabled and is used only for display purposes.
    Disabled
}

public interface IShardingNode : IPrimaryKey<int>
{
    string Name { get; init; }
    string ConnectionString { get; init; }
    Status Status { get; init; }
    string ConnectionType { get; init; }
}
