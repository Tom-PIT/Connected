using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management;

public interface IPrintManagementController
{
    ImmutableList<IPrintQueueMessage> Dequeue(int count);
    void Ping(Guid popReceipt, TimeSpan nextVisible);
    void Complete(Guid popReceipt);
    void Error(Guid popReceipt, string error);
    ImmutableList<IQueueMessage> DequeueSpooler(int count);
    void PingSpooler(Guid popReceipt, TimeSpan nextVisible);
    void CompleteSpooler(Guid popReceipt);
    Guid InsertSpooler(Guid identity, string content, string printer, string mime, long serialNumber, int copyCount);
    IPrintSpoolerJob SelectSpooler(Guid token);
    void DeleteSpooler(Guid token);
}
