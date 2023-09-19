using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class MailManagementController : IMailManagementController
{
    private const string Controller = "MailManagement";

    public void Clear()
    {
        Connection.Post(Connection.CreateUrl(Controller, "Clear"));
    }

    public void Delete(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
        {
            token
        });
    }

    public void DeleteByPopReceipt(Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteByPopReceipt"), new
        {
            popReceipt
        });
    }

    public ImmutableList<IMailMessage> Dequeue(int count)
    {
        return Connection.Post<List<MailMessage>>(Connection.CreateUrl(Controller, "Dequeue"), new
        {
            count
        }).ToImmutableList<IMailMessage>();
    }

    public ImmutableList<IMailMessage> Query()
    {
        return Connection.Get<List<MailMessage>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IMailMessage>();
    }

    public void Reset(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Reset"), new
        {
            token
        });
    }

    public IMailMessage Select(Guid token)
    {
        return Connection.Post<MailMessage>(Connection.CreateUrl(Controller, "Select"), new
        {
            token
        });
    }

    public IMailMessage SelectByPopReceipt(Guid popReceipt)
    {
        return Connection.Post<MailMessage>(Connection.CreateUrl(Controller, "SelectByPopReceipt"), new
        {
            popReceipt
        });
    }

    public void Update(Guid popReceipt, string error, int delay)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Update"), new
        {
            popReceipt,
            error,
            delay
        });
    }
}
