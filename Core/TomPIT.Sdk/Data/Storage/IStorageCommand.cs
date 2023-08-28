using System;

namespace TomPIT.Data.Storage;
public interface IStorageCommand : IDisposable, IAsyncDisposable
{
    IStorageOperation Operation { get; }
    IStorageConnection? Connection { get; }
}
