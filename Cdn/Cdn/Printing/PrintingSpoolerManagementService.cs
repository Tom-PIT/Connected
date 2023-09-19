using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
    internal class PrintingSpoolerManagementService : IPrintingSpoolerManagementService
    {
        public void Complete(Guid popReceipt)
        {
            Instance.SysProxy.Management.Printing.CompleteSpooler(popReceipt);
        }

        public List<IQueueMessage> Dequeue(int count)
        {
            return Instance.SysProxy.Management.Printing.DequeueSpooler(count).ToList();
        }

        public void Ping(Guid popReceipt)
        {
            Instance.SysProxy.Management.Printing.PingSpooler(popReceipt, TimeSpan.FromSeconds(5));
        }

        public Guid Insert(string mime, string printer, string content, long serialNumber, Guid identity, long copyCount = 1)
        {
            return Instance.SysProxy.Management.Printing.InsertSpooler(identity, content, printer, mime, serialNumber, Convert.ToInt32(copyCount));
        }
    }
}
