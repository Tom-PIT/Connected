using System;
using TomPIT.ComponentModel;
using TomPIT.Distributed;
using TomPIT.Middleware;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Deployment
{
    public class InstallerMiddleware : MiddlewareComponent, IInstallerMiddleware
    {
        public void Invoke()
        {
            Validate();
            OnInvoke();
        }

        protected virtual void OnInvoke()
        {

        }

        protected void RegisterWorker([CIP(CIP.HostedWorkerProvider)] string worker, WorkerInterval interval = WorkerInterval.Hour, int intervalValue = 1, DateTime? startDate = null,
            DateTime? endDate = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 0, int dayOfMonth = 1, WorkerDayMode dayMode = WorkerDayMode.EveryNDay,
            WorkerMonthMode monthMode = WorkerMonthMode.ExactDay, WorkerYearMode yearMode = WorkerYearMode.ExactDate, int monthNumber = 1, WorkerEndMode endMode = WorkerEndMode.NoEnd,
            WorkerCounter intervalCounter = WorkerCounter.First, WorkerMonthPart monthPart = WorkerMonthPart.Day, WorkerWeekDays weekdays = WorkerWeekDays.All, int retryInterval = 10,
            int disableTreshold = 3)
        {
            var descriptor = ComponentDescriptor.HostedWorker(Context, worker);

            descriptor.Validate();

            if (Tenant.GetService<IWorkerService>().Exists(descriptor.Component.Token))
                return;

            Tenant.GetService<IWorkerService>().Update(descriptor.Component.Token, startTime is null ? DateTime.MinValue : (DateTime)startTime, endTime is null ? DateTime.MinValue : (DateTime)endTime,
                interval, intervalValue, startDate is null ? DateTime.MinValue : (DateTime)startDate, endDate is null ? DateTime.MinValue : (DateTime)endDate, limit, dayOfMonth,
                dayMode, monthMode, yearMode, monthNumber, endMode, intervalCounter, monthPart, weekdays, WorkerKind.Worker, disableTreshold, retryInterval, WorkerStatus.Enabled, false);
        }
    }
}
