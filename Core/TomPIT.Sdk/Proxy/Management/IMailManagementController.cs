using System;
using System.Collections.Immutable;
using TomPIT.Cdn;

namespace TomPIT.Proxy.Management
{
    public interface IMailManagementController
    {
        void Clear();
        void Update(Guid popReceipt, string error, int delay);
        void Delete(Guid token);
        void DeleteByPopReceipt(Guid popReceipt);
        ImmutableList<IMailMessage> Dequeue(int count);
        ImmutableList<IMailMessage> Query();
        void Reset(Guid token);
        IMailMessage Select(Guid token);
        IMailMessage SelectByPopReceipt(Guid popReceipt);
    }
}
