using System;
using TomPIT.Cdn;

namespace TomPIT.Proxy.Remote.Management;
internal class EventDescriptor : IEventDescriptor
{
    public Guid Identifier { get; set; }

    public string Name { get; set; }

    public DateTime Created { get; set; }

    public string Arguments { get; set; }

    public string Callback { get; set; }

    public Guid MicroService { get; set; }
}
