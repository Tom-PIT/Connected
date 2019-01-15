using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Api.Storage;
using TomPIT.Caching;
using TomPIT.Services;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Workers;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Workers;

namespace TomPIT.Sys.Data
{
	internal class Workers : SynchronizedRepository<ISysScheduledJob, Guid>
	{
		public const string Queue = "worker";

		public Workers(IMemoryCache container) : base(container, "scheduledjob")
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

		public List<ISysScheduledJob> QueryScheduled()
		{
			return Where(f => f.Status == WorkerStatus.Enabled && f.NextRun != DateTime.MinValue && f.NextRun <= DateTime.UtcNow);
		}

		public void Reset(Guid worker)
		{
			var j = Get(worker);

			if (j == null)
				throw new SysException(SR.ErrWorkerNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth,
				j.DayMode, j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, WorkerStatus.Enabled,
				ScheduleCalculator.NextRun(j, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow), 0, 0, j.Logging, DateTime.MinValue,
				DateTime.MinValue, 0, j.State);

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
				j.LastComplete, j.RunCount, state);

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
				j.LastComplete, j.RunCount, j.State);

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
				lastComplete, runCount, j.State);

			Refresh(worker);
		}

		public void Update(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
			int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode,
			int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerKind kind)
		{
			var j = Get(worker);

			if (j == null)
			{
				Shell.GetService<IDatabaseService>().Proxy.Workers.Insert(worker, startTime, endTime, interval, intervalValue, startDate, endDate, limit, dayOfMonth, dayMode, monthMode, yearMode,
					monthNumber, endMode, intervalCounter, monthPart, weekdays, WorkerStatus.Disabled, DateTime.MinValue, 0, 0, false, DateTime.MinValue, DateTime.MinValue, 0, kind);
			}
			else
			{
				Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, startTime, endTime, interval, intervalValue, startDate, endDate, limit, dayOfMonth, dayMode, monthMode, yearMode,
					 monthNumber, endMode, intervalCounter, monthPart, weekdays, j.Status, j.NextRun, j.Elapsed, j.FailCount, j.Logging, j.LastRun, j.LastComplete, j.RunCount, j.State);

				if (j.Status != WorkerStatus.Disabled)
				{
					Refresh(worker);

					j = Get(worker);

					Shell.GetService<IDatabaseService>().Proxy.Workers.Update(j, j.StartTime, j.EndTime, j.Interval, j.IntervalValue, j.StartDate, j.EndDate, j.Limit, j.DayOfMonth, j.DayMode,
						j.MonthMode, j.YearMode, j.MonthNumber, j.EndMode, j.IntervalCounter, j.MonthPart, j.Weekdays, j.Status, ScheduleCalculator.NextRun(j, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow),
						j.Elapsed, j.FailCount, j.Logging, j.LastRun, j.LastComplete, j.RunCount, j.State);
				}
			}

			Refresh(worker);
		}

		public void Enqueue(ISysScheduledJob job)
		{
			Guid resourceGroup = Guid.Empty;

			if (job.Worker != Guid.Empty)
			{
				var c = DataModel.Components.Select(job.Worker);

				if (c == null)
					throw new SysException(SR.ErrComponentCorrupted);

				var s = DataModel.MicroServices.Select(c.MicroService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);

				resourceGroup = s.ResourceGroup;
			}

			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var message = new JObject
			{
				{ "worker", job.Worker },
				{ "state", job.State }
			};

			sp.Queue.Enqueue(res, Queue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			Update(job.Worker, WorkerStatus.Queued, job.NextRun, job.Elapsed,
				job.FailCount, job.LastRun, job.LastComplete, job.RunCount);
		}

		public List<IClientQueueMessage> Dequeue(IServerResourceGroup resourceGroup, int count)
		{
			var provider = Shell.GetService<IStorageProviderService>().Select(resourceGroup.StorageProvider);

			if (provider == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrStorageProviderNotRegistered, resourceGroup.StorageProvider.ToString()));

			var r = provider.Queue.DequeueSystem(resourceGroup, Queue, count);

			if (r == null)
				return r.ToClientQueueMessage(resourceGroup.Token);

			var workers = new List<IScheduledJob>();

			foreach (var i in r)
				workers.Add(Resolve(i));

			Shell.GetService<IDatabaseService>().Proxy.Workers.Dequeued(workers);

			foreach (var i in workers)
				Refresh(i.Worker);

			return r.ToClientQueueMessage(resourceGroup.Token);
		}

		public void Error(Guid microService, Guid popReceipt)
		{
			var resourceGroup = Guid.Empty;

			if (microService != Guid.Empty)
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);

				resourceGroup = s.ResourceGroup;
			}

			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = sp.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			var worker = Resolve(m);
			var setting = DataModel.Settings.Select(Guid.Empty, Settings.TaskFailTreshold);
			var treshold = 3;

			if (setting != null)
				treshold = setting.Value.AsInt();

			var status = WorkerStatus.Enabled;

			if (treshold <= worker.FailCount + 1)
				status = WorkerStatus.Disabled;

			Update(worker.Worker, status, worker.NextRun, worker.Elapsed, worker.FailCount + 1, worker.LastRun,
				worker.LastComplete, worker.RunCount);

			sp.Queue.Delete(res, popReceipt);
		}

		public void Ping(Guid microService, Guid popReceipt)
		{
			var resourceGroup = Guid.Empty;

			if (microService != Guid.Empty)
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);

				resourceGroup = s.ResourceGroup;
			}

			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			sp.Queue.Ping(res, popReceipt, TimeSpan.FromMinutes(5));
		}

		public void Complete(Guid microService, Guid popReceipt)
		{
			var resourceGroup = Guid.Empty;

			if (microService != Guid.Empty)
			{
				var s = DataModel.MicroServices.Select(microService);

				if (s == null)
					throw new SysException(SR.ErrMicroServiceNotFound);

				resourceGroup = s.ResourceGroup;
			}

			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = sp.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			var worker = Resolve(m);
			var status = WorkerStatus.Enabled;
			var elapsed = DateTime.UtcNow.Subtract(m.DequeueTimestamp).TotalMilliseconds;

			Update(worker.Worker, status, ScheduleCalculator.NextRun(worker), Convert.ToInt32(elapsed), 0,
				worker.LastRun, DateTime.UtcNow, worker.RunCount);

			sp.Queue.Delete(res, popReceipt);
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
