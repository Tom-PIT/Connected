using System;
using TomPIT.Search;

namespace TomPIT.Proxy.Remote.Management;

internal class IndexRequest : IIndexRequest
{
    public Guid Identifier { get; set; }
    public string Catalog { get; set; }
    public DateTime Created { get; set; }
    public string Arguments { get; set; }
    public Guid MicroService { get; set; }
}
