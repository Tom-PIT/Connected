using System;

namespace TomPIT.Data.Storage;
public class StorageContextArgs : EventArgs
{
    public IStorageOperation Operation { get; }

    public StorageContextArgs(IStorageOperation operation)
    {
        if (operation is null)
            throw new ArgumentException(null, nameof(operation));

        Operation = operation;
    }
}
