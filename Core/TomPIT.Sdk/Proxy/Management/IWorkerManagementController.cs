using System;
using System.Collections.Immutable;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management
{
    public interface IWorkerManagementController
    {
        ImmutableList<IQueueMessage> Dequeue(int count);
        void Ping(Guid microService, Guid popReceipt);
        void Complete(Guid microService, Guid worker, Guid popReceipt);
        void Error(Guid microService, Guid popReceipt);
        void UpdateConfiguration(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
            int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter,
            WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind, int disableTreshold, int retryInterval);
        void Update(Guid worker, WorkerStatus status, bool logging);
        void Reset(Guid worker);
        void Run(Guid worker);
        void AttachState(Guid worker, Guid state);
        IScheduledJob Select(Guid worker);
    }
}
