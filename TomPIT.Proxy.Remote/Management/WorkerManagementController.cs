using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management;
internal class WorkerManagementController : IWorkerManagementController
{
    private const string Controller = "WorkerManagement";
    public void AttachState(Guid worker, Guid state)
    {
        Connection.Post(Connection.CreateUrl(Controller, "AttachState"), new
        {
            worker,
            state
        });
    }

    public void Complete(Guid microService, Guid worker, Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Complete"), new
        {
            microService,
            popReceipt,
            worker
        });
    }

    public ImmutableList<IQueueMessage> Dequeue(int count)
    {
        return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "Dequeue"), new
        {
            count
        }).ToImmutableList<IQueueMessage>();
    }

    public void Error(Guid microService, Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Error"), new
        {
            microService,
            popReceipt
        });
    }

    public void Ping(Guid microService, Guid popReceipt)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
        {
            microService,
            popReceipt
        });
    }

    public void Reset(Guid worker)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Reset"), new
        {
            worker
        });
    }

    public void Run(Guid worker)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Run"), new
        {
            worker
        });
    }

    public void Update(Guid worker, WorkerStatus status, bool logging)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Update"), new
        {
            worker,
            status,
            logging
        });
    }

    public void UpdateConfiguration(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit, int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind, int disableTreshold, int retryInterval)
    {
        Connection.Post(Connection.CreateUrl(Controller, "UpdateConfiguration"), new
        {
            worker,
            startTime,
            endTime,
            interval,
            intervalValue,
            startDate,
            endDate,
            limit,
            dayOfMonth,
            dayMode,
            monthMode,
            yearMode,
            monthNumber,
            endMode,
            intervalCounter,
            monthPart,
            weekdays,
            kind,
            disableTreshold,
            retryInterval
        });
    }

    public IScheduledJob Select(Guid worker)
    {
        return Connection.Post<ScheduledJob>(Connection.CreateUrl(Controller, "Select"), new
        {
            worker
        });
    }
}
