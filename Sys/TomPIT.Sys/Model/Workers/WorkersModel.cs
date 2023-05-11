using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Distributed;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Workers;
using TomPIT.SysDb.Workers;

namespace TomPIT.Sys.Model.Workers
{
   public class WorkersModel : SynchronizedRepository<ISysScheduledJob, Guid>
   {
      public const string Queue = "worker";

      public WorkersModel(IMemoryCache container) : base(container, "scheduledjob")
      {
      }

      protected override void OnInitializing()
      {
         var ds = Shell.GetService<IDatabaseService>().Proxy.Workers.Query();

         foreach (var i in ds)
            Set(i.Worker, i, TimeSpan.Zero);
      }

      protected override void OnInvalidate(Guid id)
      {
         var d = Shell.GetService<IDatabaseService>().Proxy.Workers.Select(id);

         if (d == null)
         {
            Remove(id);
            return;
         }

         Set(id, d, TimeSpan.Zero);
      }

      public ImmutableList<ISysScheduledJob> QueryScheduled()
      {
         return Where(f => (f.Status == WorkerStatus.Enabled) && f.NextRun != DateTime.MinValue && f.NextRun <= DateTime.UtcNow);
      }

      public ImmutableList<ISysScheduledJob> QueryQueued()
      {
         return Where(f => f.Status == WorkerStatus.Queued);
      }

      public void Reset(Guid worker)
      {
         var j = Get(worker);

         if (j == null)
            throw new SysException(SR.ErrWorkerNotFound);

         Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth,
            j.DayMode, j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, WorkerStatus.Enabled,
            ScheduleCalculator.NextRun(j, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow), 0, 0, j.Logging, DateTime.MinValue,
            DateTime.MinValue, 0, j.State, j.RetryInterval, j.DisableTreshold);

         Refresh(worker);
      }

      public void Run(Guid worker)
      {
         var j = Get(worker);

         if (j == null)
            throw new SysException(SR.ErrWorkerNotFound);

         Enqueue(j);
      }

      public void UpdateState(Guid worker, Guid state)
      {
         var j = Get(worker);

         if (j == null)
            throw new SysException(SR.ErrWorkerNotFound);

         var nextRun = j.NextRun == DateTime.MinValue
            ? ScheduleCalculator.NextRun(j, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow)
            : j.NextRun;

         Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth,
            j.DayMode, j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, j.Status, nextRun, j.Elapsed, j.FailCount, j.Logging, j.LastRun,
            j.LastComplete, j.RunCount, state, j.RetryInterval, j.DisableTreshold);

         Refresh(worker);
      }

      public void Update(Guid worker, WorkerStatus status, bool logging)
      {
         if (status == WorkerStatus.Queued)
            throw new SysException(string.Format("{0} ({1})", SR.ErrWorkerStatusNotAllowed, status));

         var j = Get(worker);

         if (j == null)
            throw new SysException(SR.ErrWorkerNotFound);

         if (j.Status != status && j.Status == WorkerStatus.Queued)
            throw new SysException(SR.ErrWorkerQueued);

         var nextRun = j.NextRun == DateTime.MinValue
            ? ScheduleCalculator.NextRun(j, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow)
            : j.NextRun;

         Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth,
            j.DayMode, j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, status, nextRun, j.Elapsed, j.FailCount, logging, j.LastRun,
            j.LastComplete, j.RunCount, j.State, j.RetryInterval, j.DisableTreshold);

         Refresh(worker);
      }

      public void Update(Guid worker, WorkerStatus status, DateTime nextRun, int elapsed,
         int failCount, DateTime lastRun, DateTime lastComplete, long runCount)
      {
         var j = Get(worker);

         if (j == null)
            throw new SysException(SR.ErrWorkerNotFound);

         Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth,
            j.DayMode, j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, status, nextRun, elapsed, failCount, j.Logging, lastRun,
            lastComplete, runCount, j.State, j.RetryInterval, j.DisableTreshold);

         Refresh(worker);
      }

      public void Update(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
         int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode,
         int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind, int disableTreshold, int retryInterval)
      {
         var j = Get(worker);

         if (j == null)
         {
            Shell.GetService<IDatabaseService>().Proxy.Workers.Insert(worker, startTime, endTime, interval, intervalValue, startDate, endDate, limit, dayOfMonth, dayMode, monthMode, yearMode,
               monthNumber, endMode, intervalCounter, monthPart, weekdays, WorkerStatus.Disabled, DateTime.MinValue, 0, 0, false, DateTime.MinValue, DateTime.MinValue, 0, kind, retryInterval, disableTreshold);
         }
         else
         {
            Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, startTime, endTime, interval, intervalValue, startDate, endDate, limit, dayOfMonth, dayMode, monthMode, yearMode,
                monthNumber, endMode, intervalCounter, monthPart, weekdays, j.Status, j.NextRun, j.Elapsed, j.FailCount, j.Logging, j.LastRun, j.LastComplete, j.RunCount, j.State, retryInterval, disableTreshold);

            if (j.Status != WorkerStatus.Disabled)
            {
               Refresh(worker);

               j = Get(worker);

               Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth, j.DayMode,
                  j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, j.Status, ScheduleCalculator.NextRun(j, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow),
                  j.Elapsed, j.FailCount, j.Logging, j.LastRun, j.LastComplete, j.RunCount, j.State, retryInterval, disableTreshold);
            }
         }

         Refresh(worker);
      }

      public void Enqueue(ISysScheduledJob job)
      {
         var message = new JObject
         {
            { "worker", job.Worker },
            { "state", job.State }
         };

         DataModel.Queue.Enqueue(Queue, JsonConvert.SerializeObject(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

         Update(job.Worker, WorkerStatus.Queued, job.NextRun, job.Elapsed,
            job.FailCount, job.LastRun, job.LastComplete, job.RunCount);
      }

      public ImmutableList<IQueueMessage> Dequeue(int count)
      {
         var r = DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(5), QueueScope.System, Queue);

         if (r == null)
            return null;

         var workers = new List<IScheduledJob>();

         foreach (var i in r)
            workers.Add(Resolve(i));

         Shell.GetService<IDatabaseService>().Proxy.Workers.Dequeued(workers);

         foreach (var i in workers)
            Refresh(i.Worker);

         return r;
      }

      public void Error(Guid microService, Guid popReceipt)
      {
         if (DataModel.Queue.Select(popReceipt) is not IQueueMessage message)
            return;

         var worker = Resolve(message);
         var status = WorkerStatus.Enabled;

         if (worker.DisableTreshold <= worker.FailCount + 1)
            status = WorkerStatus.Disabled;

         Update(worker.Worker, status, DateTime.UtcNow.AddSeconds(worker.RetryInterval), worker.Elapsed, worker.FailCount + 1, worker.LastRun, worker.LastComplete, worker.RunCount);

         DataModel.Queue.Complete(popReceipt);
      }

      public void Ping(Guid microService, Guid popReceipt)
      {
         DataModel.Queue.Ping(popReceipt, TimeSpan.FromMinutes(5));
      }

      public void Complete(Guid microService, Guid popReceipt, Guid worker)
      {
         var queueEntry = DataModel.Queue.Select(popReceipt);

         if (queueEntry is not null)
            DataModel.Queue.Complete(popReceipt);

         var w = Select(worker);
         var status = WorkerStatus.Enabled;
         var elapsed = DateTime.UtcNow.Subtract(queueEntry?.DequeueTimestamp ?? DateTime.UtcNow).TotalMilliseconds;

         Update(w.Worker, status, ScheduleCalculator.NextRun(w), Convert.ToInt32(elapsed), 0,
            w.LastRun, DateTime.UtcNow, w.RunCount);
      }

      public ISysScheduledJob Select(Guid worker)
      {
         return Get(worker);
      }

      private ISysScheduledJob Resolve(IQueueMessage message)
      {
         var d = JsonConvert.DeserializeObject(message.Message) as JObject;
         var p = d.Required<Guid>("worker");
         var worker = Select(p);

         if (worker == null)
            throw new SysException(SR.ErrWorkerNotFound);

         return worker;
      }
   }
}
