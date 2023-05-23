using System;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class MailManagementController : IMailManagementController
{
    public void Clear()
    {
        DataModel.Mail.Clear();
    }

    public void Delete(Guid token)
    {
        DataModel.Mail.Delete(token);
    }

    public void DeleteByPopReceipt(Guid popReceipt)
    {
        DataModel.Mail.DeleteByPopReceipt(popReceipt);
    }

    public ImmutableList<IMailMessage> Dequeue(int count)
    {
        return DataModel.Mail.Dequeue(count).ToImmutableList();
    }

    public ImmutableList<IMailMessage> Query()
    {
        return DataModel.Mail.Query().ToImmutableList();
    }

    public void Reset(Guid token)
    {
        DataModel.Mail.Reset(token);
    }

    public IMailMessage Select(Guid token)
    {
        return DataModel.Mail.Select(token);
    }

    public IMailMessage SelectByPopReceipt(Guid popReceipt)
    {
        return DataModel.Mail.SelectByPopReceipt(popReceipt);
    }

    public void Update(Guid popReceipt, string error, int delay)
    {
        DataModel.Mail.Response(popReceipt, error, delay);
    }
}
