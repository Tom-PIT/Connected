using System;
using TomPIT.Data.Sql;
using TomPIT.Services;

namespace TomPIT.SysDb.Sql.Workers
{
	internal class ScheduledJob : PrimaryKeyRecord, IScheduledJob
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public WorkerInterval Interval { get; set; }
		public int IntervalValue { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Limit { get; set; }
		public WorkerStatus Status { get; set; }
		public int DayOfMonth { get; set; }
		public DateTime LastRun { get; set; }
		public DateTime LastComplete { get; set; }
		public DateTime NextRun { get; set; }
		public WorkerDayMode DayMode { get; set; }
		public WorkerMonthMode MonthMode { get; set; }
		public WorkerYearMode YearMode { get; set; }
		public long RunCount { get; set; }
		public int MonthNumber { get; set; }
		public WorkerEndMode EndMode { get; set; }
		public WorkerCounter IntervalCounter { get; set; }
		public WorkerMonthPart MonthPart { get; set; }
		public int Elapsed { get; set; }
		public WorkerWeekDays Weekdays { get; set; }
		public Guid Api { get; set; }
		public Guid Operation { get; set; }
		public int FailCount { get; set; }
		public Guid MicroService { get; set; }
		public bool Logging { get; set; }
		public WorkerKind Kind { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			StartTime = GetDate("start_time");
			EndTime = GetDate("end_time");
			Interval = GetValue("interval_type", WorkerInterval.Once);
			IntervalValue = GetInt("interval_value");
			StartDate = GetDate("start_date");
			EndDate = GetDate("end_date");
			Limit = GetInt("limit");
			Status = GetValue("status", WorkerStatus.Disabled);
			DayOfMonth = GetInt("day_of_month");
			LastRun = GetDate("last_run");
			LastComplete = GetDate("last_complete");
			NextRun = GetDate("next_run");
			DayMode = GetValue("day_mode", WorkerDayMode.EveryNDay);
			MonthMode = GetValue("month_mode", WorkerMonthMode.ExactDay);
			YearMode = GetValue("year_mode", WorkerYearMode.ExactDate);
			RunCount = GetLong("run_count");
			MonthNumber = GetInt("month_number");
			EndMode = GetValue("end_mode", WorkerEndMode.NoEnd);
			IntervalCounter = GetValue("interval_counter", WorkerCounter.First);
			MonthPart = GetValue("month_part", WorkerMonthPart.Day);
			Elapsed = GetInt("elapsed");
			Weekdays = GetValue("weekdays", WorkerWeekDays.All);
			Api = GetGuid("api");
			Operation = GetGuid("operation");
			FailCount = GetInt("fail_count");
			MicroService = GetGuid("service");
			Logging = GetBool("logging");
			Kind = GetValue("kind", WorkerKind.Worker);
		}
	}
}
