using System;

namespace TomPIT.Storage;

public interface IClientStorageProvider
{
    Guid Token { get; }
    string Name { get; }
}
