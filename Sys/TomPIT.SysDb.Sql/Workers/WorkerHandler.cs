using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Distributed;
using TomPIT.SysDb.Workers;

namespace TomPIT.SysDb.Sql.Workers
{
	internal class WorkerHandler : IWorkerHandler
	{
		public void Delete(IScheduledJob job)
		{
			var w = new Writer("tompit.worker_del");

			w.CreateParameter("@id", job.Id);

			w.Execute();
		}

		public void Dequeued(List<IScheduledJob> workers)
		{
			var w = new Writer("tompit.worker_upd_stats");

			w.CreateParameter("@id", 0);
			w.CreateParameter("@last_run", DateTime.UtcNow);
			w.CreateParameter("@run_count", 0);

			w.Prepare();

			foreach (var i in workers)
			{
				w.ModifyParameter("@id", i.GetId());
				w.ModifyParameter("@run_count", i.RunCount + 1);

				w.Execute();
			}

			w.Complete();
		}

		public void Insert(Guid worker, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate,
			int limit, int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter,
			WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerStatus status, DateTime nextRun, int elapsed, int failCount, bool logging, DateTime lastRun, DateTime lastComplete,
			long runCount, WorkerKind kind)
		{
			var w = new Writer("tompit.worker_ins");

			w.CreateParameter("@worker", worker);
			w.CreateParameter("@start_time", startTime, true);
			w.CreateParameter("@end_time", endTime, true);
			w.CreateParameter("@interval_type", interval);
			w.CreateParameter("@interval_value", intervalValue);
			w.CreateParameter("@start_date", startDate, true);
			w.CreateParameter("@end_date", endDate, true);
			w.CreateParameter("@limit", limit);
			w.CreateParameter("@day_of_month", dayOfMonth);
			w.CreateParameter("@day_mode", dayMode);
			w.CreateParameter("@month_mode", monthMode);
			w.CreateParameter("@year_mode", yearMode);
			w.CreateParameter("@month_number", monthNumber);
			w.CreateParameter("@end_mode", endMode);
			w.CreateParameter("@interval_counter", intervalCounter);
			w.CreateParameter("@month_part", monthPart);
			w.CreateParameter("@weekdays", weekdays);
			w.CreateParameter("@status", status);
			w.CreateParameter("@next_run", nextRun, true);
			w.CreateParameter("@elapsed", elapsed);
			w.CreateParameter("@fail_count", failCount);
			w.CreateParameter("@logging", logging);
			w.CreateParameter("@last_run", lastRun, true);
			w.CreateParameter("@last_complete", lastComplete, true);
			w.CreateParameter("@run_count", runCount);
			w.CreateParameter("@kind", kind);

			w.Execute();
		}

		public List<ISysScheduledJob> Query()
		{
			return new Reader<ScheduledJob>("tompit.worker_que").Execute().ToList<ISysScheduledJob>();
		}

		public ISysScheduledJob Select(Guid worker)
		{
			var r = new Reader<ScheduledJob>("tompit.worker_sel");

			r.CreateParameter("@worker", worker);

			return r.ExecuteSingleRow();
		}

		public void Update(IScheduledJob job, DateTime startTime, DateTime endTime, WorkerInterval interval, int intervalValue, DateTime startDate, DateTime endDate,
			int limit, int dayOfMonth, WorkerDayMode dayMode, WorkerMonthMode monthMode, WorkerYearMode yearMode, int monthNumber, WorkerEndMode endMode, WorkerCounter intervalCounter,
			WorkerMonthPart monthPart, WorkerWeekDays weekdays, WorkerStatus status, DateTime nextRun, int elapsed, int failCount, bool logging, DateTime lastRun, DateTime lastComplete,
			long runCount, Guid state)
		{
			var w = new Writer("tompit.worker_upd");

			w.CreateParameter("@id", job.Id);
			w.CreateParameter("@start_time", startTime, true);
			w.CreateParameter("@end_time", endTime, true);
			w.CreateParameter("@interval_type", interval);
			w.CreateParameter("@interval_value", intervalValue);
			w.CreateParameter("@start_date", startDate, true);
			w.CreateParameter("@end_date", endDate, true);
			w.CreateParameter("@limit", limit);
			w.CreateParameter("@day_of_month", dayOfMonth);
			w.CreateParameter("@day_mode", dayMode);
			w.CreateParameter("@month_mode", monthMode);
			w.CreateParameter("@year_mode", yearMode);
			w.CreateParameter("@month_number", monthNumber);
			w.CreateParameter("@end_mode", endMode);
			w.CreateParameter("@interval_counter", intervalCounter);
			w.CreateParameter("@month_part", monthPart);
			w.CreateParameter("@weekdays", weekdays);
			w.CreateParameter("@status", status);
			w.CreateParameter("@next_run", nextRun, true);
			w.CreateParameter("@elapsed", elapsed);
			w.CreateParameter("@fail_count", failCount);
			w.CreateParameter("@logging", logging);
			w.CreateParameter("@last_run", lastRun, true);
			w.CreateParameter("@last_complete", lastComplete, true);
			w.CreateParameter("@run_count", runCount);
			w.CreateParameter("@state", state, true);

			w.Execute();
		}
	}
}
