using System;
using TomPIT.Connectivity;

namespace TomPIT.Distributed
{
    internal class WorkerService : TenantObject, IWorkerService
    {
        public WorkerService(ITenant tenant) : base(tenant)
        {

        }

        public bool Exists(Guid configuration)
        {
            return Instance.SysProxy.Management.Workers.Select(configuration) is not null;
        }

        public void Run(Guid worker)
        {
            Instance.SysProxy.Management.Workers.Run(worker);
        }

        public void Update(Guid configuration, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit, int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind, int disableTreshold, int retryInterval, WorkerStatus status, bool logging)
        {
            Instance.SysProxy.Management.Workers.UpdateConfiguration(configuration, startTime, endTime, interval, intervalValue, startDate, endDate, limit,
                dayOfMonth, dayMode, monthMode, yearMode, monthNumber, endMode, intervalCounter, monthPart, weekdays, kind, disableTreshold, retryInterval);
            Instance.SysProxy.Management.Workers.Update(configuration, status, logging);
        }
    }
}
