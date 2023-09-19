using System;

namespace TomPIT.Data.Storage;
public sealed class StorageConnectionArgs : EventArgs
{
    public StorageConnectionArgs(string connectionString, StorageConnectionMode behavior)
    {
        ConnectionString = connectionString;
        Behavior = behavior;
    }

    public string ConnectionString { get; }
    public StorageConnectionMode Behavior { get; }
}
