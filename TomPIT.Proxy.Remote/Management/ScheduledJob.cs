using System;
using TomPIT.Distributed;

namespace TomPIT.Proxy.Remote.Management;
internal class ScheduledJob : IScheduledJob
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

    public Guid Worker { get; set; }

    public int FailCount { get; set; }

    public bool Logging { get; set; }

    public WorkerKind Kind { get; set; }

    public int RetryInterval { get; set; }

    public int DisableTreshold { get; set; }

    public int Id { get; set; }
}
