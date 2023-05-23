using System;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Cdn.Printing
{
    internal class PrintingManagementService : IPrintingManagementService
    {
        public void Complete(Guid popReceipt)
        {
            Instance.SysProxy.Management.Printing.Complete(popReceipt);
        }

        public List<IPrintQueueMessage> Dequeue(int count)
        {
            return Instance.SysProxy.Management.Printing.Dequeue(count).ToList();
        }

        public void Error(Guid popReceipt, string error)
        {
            Instance.SysProxy.Management.Printing.Error(popReceipt, error);
        }

        public void Ping(Guid popReceipt)
        {
            Instance.SysProxy.Management.Printing.Ping(popReceipt, TimeSpan.FromMinutes(4));
        }
    }
}
