using System;
using TomPIT.Cdn;
using TomPIT.Distributed;

namespace TomPIT.Proxy.Remote.Management;

internal class PrintQueueMessage : QueueMessage, IPrintQueueMessage
{
    public Guid Token { get; set; }

    public PrintJobStatus Status { get; set; }

    public string Error { get; set; }

    public string Provider { get; set; }
    public string Arguments { get; set; }
    public Guid Component { get; set; }

    public string User { get; set; }

    public long SerialNumber { get; set; }

    public string Category { get; set; }

    public int CopyCount { get; set; }
}
