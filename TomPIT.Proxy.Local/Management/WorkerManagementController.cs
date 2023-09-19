using System;
using System.Collections.Immutable;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class WorkerManagementController : IWorkerManagementController
{
    public void AttachState(Guid worker, Guid state)
    {
        DataModel.Workers.UpdateState(worker, state);
    }

    public void Complete(Guid microService, Guid worker, Guid popReceipt)
    {
        DataModel.Workers.Complete(microService, popReceipt, worker);
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return DataModel.Workers.Dequeue(count);
    }

    public void Error(Guid microService, Guid popReceipt)
    {
        DataModel.Workers.Error(microService, popReceipt);
    }

    public void Ping(Guid microService, Guid popReceipt)
    {
        DataModel.Workers.Ping(microService, popReceipt);
    }

    public void Reset(Guid worker)
    {
        DataModel.Workers.Reset(worker);
    }

    public void Run(Guid worker)
    {
        DataModel.Workers.Run(worker);
    }

    public void Update(Guid worker, WorkerStatus status, bool logging)
    {
        DataModel.Workers.Update(worker, status, logging);
    }

    public void UpdateConfiguration(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit, int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind, int disableTreshold, int retryInterval)
    {
        DataModel.Workers.Update(worker, startTime, endTime, interval, intervalValue, startDate, endDate, limit, dayOfMonth, dayMode, monthMode, yearMode, monthNumber, endMode,
            intervalCounter, monthPart, weekdays, kind, disableTreshold, retryInterval);
    }

    public IScheduledJob Select(Guid worker)
    {
        return DataModel.Workers.Select(worker);
    }
}
