using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Proxy.Management;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class PrintManagementController : IPrintManagementController
{
    public void Complete(Guid popReceipt)
    {
        DataModel.Printing.Complete(popReceipt);
    }

    public void CompleteSpooler(Guid popReceipt)
    {
        DataModel.PrintingSpooler.Complete(popReceipt);
    }

    public void DeleteSpooler(Guid token)
    {
        DataModel.PrintingSpooler.Delete(token);
    }

    public ImmutableList<IPrintQueueMessage> Dequeue(int count)
    {
        return DataModel.Printing.Dequeue(count);
    }

    public ImmutableList<IQueueMessage> DequeueSpooler(int count)
    {
        return DataModel.PrintingSpooler.Dequeue(count);
    }

    public void Error(Guid popReceipt, string error)
    {
        DataModel.Printing.Error(popReceipt, error);
    }

    public Guid InsertSpooler(Guid identity, string content, string printer, string mime, long serialNumber, int copyCount = 1)
    {
        return DataModel.PrintingSpooler.Insert(mime, printer, content, serialNumber, identity, copyCount);
    }

    public void Ping(Guid popReceipt, TimeSpan nextVisible)
    {
        DataModel.Printing.Ping(popReceipt, nextVisible);
    }

    public void PingSpooler(Guid popReceipt, TimeSpan nextVisible)
    {
        DataModel.PrintingSpooler.Ping(popReceipt, nextVisible);
    }

    public IPrintSpoolerJob SelectSpooler(Guid token)
    {
        return DataModel.PrintingSpooler.Select(token);
    }
}
