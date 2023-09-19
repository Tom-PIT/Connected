using System;
using TomPIT.Search;

namespace TomPIT.Proxy.Remote.Management;

internal class CatalogState : ICatalogState
{
    public Guid Catalog { get; set; }
    public CatalogStateStatus Status { get; set; }
}
