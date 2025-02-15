﻿using System;
using System.Collections.Generic;
using TomPIT.Distributed;

namespace TomPIT.SysDb.Workers
{
	public interface IWorkerHandler
	{
		void Insert(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
			int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode,
			int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerStatus status, DateTime nextRun, int elapsed,
			int failCount, bool logging, DateTime lastRun, DateTime lastComplete, long runCount, WorkerKind kind, int retryInterval, int disableTreshold);

		void Update(IScheduledJob job, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate, int limit,
			int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode,
			int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter, WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerStatus status, DateTime nextRun, int elapsed,
			int failCount, bool logging, DateTime lastRun, DateTime lastComplete, long runCount, Guid state, int retryInterval, int disableTreshold);

		List<ISysScheduledJob> Query();
		ISysScheduledJob Select(Guid worker);
		void Delete(IScheduledJob job);

		void Dequeued(List<IScheduledJob> workers);
	}
}
