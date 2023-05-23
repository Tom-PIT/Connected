using System;

namespace TomPIT.Distributed
{
    public interface IWorkerService
    {
        void Run(Guid configuration);
        void Update(Guid configuration, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate,
            int limit, int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode,
            WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind, int disableTreshold, int retryInterval,
            WorkerStatus status, bool logging);
        bool Exists(Guid configuration);
    }
}
