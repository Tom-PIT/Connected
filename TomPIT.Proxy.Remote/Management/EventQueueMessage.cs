using System;
using TomPIT.Cdn;
using TomPIT.Distributed;

namespace TomPIT.Proxy.Remote.Management;

internal class EventQueueMessage : QueueMessage, IEventQueueMessage
{
    public string Arguments { get; set; }
    public string Name { get; set; }
    public string Callback { get; set; }
    public Guid MicroService { get; set; }
}
