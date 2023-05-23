using System;
using TomPIT.Connectivity;

namespace TomPIT.Worker.Services;

internal class WorkerProxyService : TenantObject, IWorkerProxyService
{
    public WorkerProxyService(ITenant tenant) : base(tenant)
    {
    }

    public void Ping(Guid microService, Guid popReceipt)
    {
        Instance.SysProxy.Management.Workers.Ping(microService, popReceipt);
    }

    public void Error(Guid microService, Guid popReceipt)
    {
        Instance.SysProxy.Management.Workers.Error(microService, popReceipt);
    }

    public void Complete(Guid microService, Guid popReceipt, Guid worker)
    {
        Instance.SysProxy.Management.Workers.Complete(microService, worker, popReceipt);
    }

    public void AttachState(Guid worker, Guid state)
    {
        Instance.SysProxy.Management.Workers.AttachState(worker, state);
    }
}
