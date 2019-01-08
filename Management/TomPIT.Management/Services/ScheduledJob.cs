using System;

namespace TomPIT.Services
{
	internal class ScheduledJob : IScheduledJob
	{
		public DateTime StartTime { get; set; } = DateTime.MinValue;
		public DateTime EndTime { get; set; } = DateTime.MinValue;
		public WorkerInterval Interval { get; set; } = WorkerInterval.Day;
		public int IntervalValue { get; set; } = 1;
		public DateTime StartDate { get; set; } = DateTime.MinValue;
		public DateTime EndDate { get; set; } = DateTime.MinValue;
		public int Limit { get; set; } = 0;
		public WorkerStatus Status { get; set; } = WorkerStatus.Disabled;
		public int DayOfMonth { get; set; } = 1;
		public DateTime LastRun { get; set; } = DateTime.MinValue;
		public DateTime LastComplete { get; set; } = DateTime.MinValue;
		public DateTime NextRun { get; set; } = DateTime.MinValue;
		public WorkerDayMode DayMode { get; set; } = WorkerDayMode.EveryNDay;
		public WorkerMonthMode MonthMode { get; set; } = WorkerMonthMode.ExactDay;
		public WorkerYearMode YearMode { get; set; } = WorkerYearMode.ExactDate;
		public long RunCount { get; set; } = 0;
		public int MonthNumber { get; set; } = 1;
		public WorkerEndMode EndMode { get; set; } = WorkerEndMode.NoEnd;
		public WorkerCounter IntervalCounter { get; set; } = WorkerCounter.First;
		public WorkerMonthPart MonthPart { get; set; } = WorkerMonthPart.Weekday;
		public int Elapsed { get; set; } = 0;
		public WorkerWeekDays Weekdays { get; set; } = WorkerWeekDays.None;
		public Guid Api { get; set; } = Guid.Empty;
		public Guid Operation { get; set; } = Guid.Empty;
		public int FailCount { get; set; } = 0;
		public Guid MicroService { get; set; } = Guid.Empty;
		public bool Logging { get; set; } = false;
		public WorkerKind Kind { get; set; } = WorkerKind.Worker;
		public int Id { get; set; } = 0;
	}
}
