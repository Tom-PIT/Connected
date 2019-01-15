using System;
using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.SysDb.Workers
{
	public interface IWorkerHandler
	{
		void Insert(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
			int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode,
			int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerStatus status, DateTime nextRun, int elapsed,
			int failCount, bool logging, DateTime lastRun, DateTime lastComplete, long runCount, WorkerKind kind);

		void Update(IScheduledJob job, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
			int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode,
			int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerStatus status, DateTime nextRun, int elapsed,
			int failCount, bool logging, DateTime lastRun, DateTime lastComplete, long runCount, Guid state);

		List<ISysScheduledJob> Query();
		ISysScheduledJob Select(Guid worker);
		void Delete(IScheduledJob job);

		void Dequeued(List<IScheduledJob> workers);
	}
}
