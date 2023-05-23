using System;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management;

internal class ClientStorageProvider : IClientStorageProvider
{
    public Guid Token { get; set; }
    public string Name { get; set; }
}
