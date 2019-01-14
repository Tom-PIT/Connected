using System;
using TomPIT.Data;

namespace TomPIT.Services
{
	public enum WorkerInterval
	{
		Once = 1,
		Second = 2,
		Minute = 3,
		Hour = 4,
		Day = 5,
		Week = 6,
		Month = 7,
		Year = 8
	}

	public enum WorkerStatus
	{
		Enabled = 1,
		Queued = 2,
		Disabled = 3
	}

	public enum WorkerDayMode
	{
		EveryNDay = 1,
		EveryWeekday = 2
	}

	public enum WorkerMonthMode
	{
		ExactDay = 1,
		RelativeDay = 2
	}

	public enum WorkerYearMode
	{
		ExactDate = 1,
		RelativeDate = 2
	}

	public enum WorkerEndMode
	{
		NoEnd = 1,
		Occurrence = 2,
		Date = 3
	}

	public enum WorkerMonthPart
	{
		Monday = 1,
		Tuesday = 2,
		Wednesday = 3,
		Thursday = 4,
		Friday = 5,
		Saturday = 6,
		Sunday = 7,
		Weekday = 8,
		WeekendDay = 9,
		Day = 10,
	}

	public enum WorkerCounter
	{
		First = 1,
		Second = 2,
		Third = 3,
		Fourth = 4,
		Last = 5
	}

	[Flags]
	public enum WorkerWeekDays
	{
		None = 0,
		Monday = 1,
		Tuesday = 2,
		Wednesday = 4,
		Thursday = 8,
		Friday = 16,
		Saturday = 32,
		Sunday = 64,
		All = 127
	}

	public enum WorkerKind
	{
		Worker = 1
	}

	public interface IScheduledJob : IPrimaryKeyRecord
	{
		DateTime StartTime { get; }
		DateTime EndTime { get; }
		WorkerInterval Interval { get; }
		int IntervalValue { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		int Limit { get; }
		WorkerStatus Status { get; }
		int DayOfMonth { get; }
		DateTime LastRun { get; }
		DateTime LastComplete { get; }
		DateTime NextRun { get; }
		WorkerDayMode DayMode { get; }
		WorkerMonthMode MonthMode { get; }
		WorkerYearMode YearMode { get; }
		long RunCount { get; }
		int MonthNumber { get; }
		WorkerEndMode EndMode { get; }
		WorkerCounter IntervalCounter { get; }
		WorkerMonthPart MonthPart { get; }
		int Elapsed { get; }
		WorkerWeekDays Weekdays { get; }
		Guid Worker { get; }
		int FailCount { get; }
		bool Logging { get; }
		WorkerKind Kind { get; }
	}
}
