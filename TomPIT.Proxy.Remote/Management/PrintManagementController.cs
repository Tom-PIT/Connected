using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management;
internal class PrintManagementController : IPrintManagementController
{
    private const string Controller = "PrintingManagement";
    public void Complete(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Complete"), new
        {
            popReceipt
        });
    }

    public void CompleteSpooler(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "CompleteSpooler"), new
        {
            popReceipt
        });
    }

    public void DeleteSpooler(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteSpooler"), new
        {
            token
        });
    }

    public ImmutableList<IPrintQueueMessage> Dequeue(int count)
    {
        return Connection.Post<List<PrintQueueMessage>>(Connection.CreateUrl(Controller, "Dequeue"), new
        {
            count
        }).ToImmutableList<IPrintQueueMessage>();
    }

    public ImmutableList<IQueueMessage> DequeueSpooler(int count)
    {
        return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "DequeueSpooler"), new
        {
            count
        }).ToImmutableList<IQueueMessage>();
    }

    public void Error(Guid popReceipt, string error)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Error"), new
        {
            popReceipt,
            error
        });
    }

    public Guid InsertSpooler(Guid identity, string content, string printer, string mime, long serialNumber, int copyCount)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertSpooler"), new
        {
            mime,
            printer,
            content,
            serialNumber,
            identity,
            copyCount
        });
    }

    public void Ping(Guid popReceipt, TimeSpan nextVisible)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
        {
            popReceipt,
            nextVisible
        });
    }

    public void PingSpooler(Guid popReceipt, TimeSpan nextVisible)
    {
        Connection.Post(Connection.CreateUrl(Controller, "PingSpooler"), new
        {
            popReceipt,
            nextVisible
        });
    }

    public IPrintSpoolerJob SelectSpooler(Guid token)
    {
        return Connection.Post<PrintSpoolerJob>(Connection.CreateUrl(Controller, "SelectSpooler"), new
        {
            token
        });
    }
}
