using System;

namespace TomPIT.Environment;

public interface IServerResourceGroup : IResourceGroup
{
    Guid StorageProvider { get; }
    string ConnectionString { get; }
}
