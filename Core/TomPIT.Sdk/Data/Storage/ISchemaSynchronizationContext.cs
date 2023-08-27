using System;

namespace TomPIT.Data.Storage;
public interface ISchemaSynchronizationContext
{
    Type ConnectionType { get; }
    string ConnectionString { get; }
}
