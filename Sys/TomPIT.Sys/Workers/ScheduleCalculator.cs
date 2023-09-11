using System;

using TomPIT.Distributed;

namespace TomPIT.Sys.Workers
{
	internal static class ScheduleCalculator
	{
		public static DateTime NextRun(IScheduledJob worker)
		{
			var initial = worker.NextRun;
			var now = DateTime.UtcNow;

			if (initial > now)
				return initial;

			DateTime result;
			do
			{
				result = NextRun(worker, initial, now);

				if (result == DateTime.MinValue)
					return result;

				initial = result;

			} while (result < now);

			return result;
		}

		public static DateTime NextRun(IScheduledJob worker, DateTime initial, DateTime now)
		{
			try
			{
				now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

				switch (worker.Interval)
				{
					case WorkerInterval.Second:
						return CalcNextRunSecond(worker, initial, now);
					case WorkerInterval.Minute:
						return CalcNextRunMinute(worker, initial, now);
					case WorkerInterval.Day:
						return CalcNextRunDialy(worker, initial, now);
					case WorkerInterval.Hour:
						return CalcNextRunHourly(worker, initial, now);
					case WorkerInterval.Month:
						return CalcNextRunMonthly(worker, initial, now);
					case WorkerInterval.Once:
						return CalcNextRunOnce(worker, initial, now);
					case WorkerInterval.Week:
						return CalcNextRunWeekly(worker, initial, now);
					case WorkerInterval.Year:
						return CalcNextRunYearly(worker, initial, now);
					default:
						return DateTime.MinValue;
				}
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		private static DateTime CorrectStart(IScheduledJob worker, DateTime date, DateTime now)
		{
			if (date == DateTime.MinValue)
				date = now;

			if (worker.StartDate == DateTime.MinValue || worker.StartDate.Date <= date.Date)
			{
				if (worker.StartTime == DateTime.MinValue)
					return date;
				else
				{
					if (worker.StartTime.TimeOfDay > date.TimeOfDay)
						return new DateTime(date.Year, date.Month, date.Day, worker.StartTime.Hour, worker.StartTime.Minute, worker.StartTime.Second);
					else
						return date;
				}
			}
			else
			{
				if (worker.StartTime == DateTime.MinValue)
					return new DateTime(worker.StartDate.Year, worker.StartDate.Month, worker.StartDate.Day, date.Hour, date.Minute, date.Second);
				else
				{
					if (worker.StartTime.TimeOfDay > date.TimeOfDay)
						return new DateTime(worker.StartDate.Year, worker.StartDate.Month, worker.StartDate.Day, worker.StartTime.Hour, worker.StartTime.Minute, worker.StartTime.Second);
					else
						return new DateTime(worker.StartDate.Year, worker.StartDate.Month, worker.StartDate.Day, date.Hour, date.Minute, date.Second);
				}
			}
		}

		private static DateTime CalcNextRunOnce(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (initial != DateTime.MinValue)
				return EnsureValidDate(worker, DateTime.MinValue);
			else
				return EnsureValidDate(worker, CorrectStart(worker, worker.LastRun, now));
		}

		private static DateTime CalcNextRunSecond(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;

			var nextRun = initial.AddSeconds(worker.IntervalValue);

			nextRun = CorrectStart(worker, nextRun, now);

			if (worker.EndTime != DateTime.MinValue && worker.EndTime.TimeOfDay < nextRun.TimeOfDay)
				nextRun = CorrectTime(nextRun.AddDays(1), worker.StartTime);

			return EnsureValidDate(worker, nextRun);
		}

		private static DateTime CalcNextRunMinute(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;

			var nextRun = initial.AddMinutes(worker.IntervalValue);

			nextRun = CorrectStart(worker, nextRun, now);

			if (worker.EndTime != DateTime.MinValue && worker.EndTime.TimeOfDay < nextRun.TimeOfDay)
				nextRun = CorrectTime(nextRun.AddDays(1), worker.StartTime);

			return EnsureValidDate(worker, nextRun);
		}

		private static DateTime CalcNextRunHourly(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;

			var nextRun = initial.AddHours(worker.IntervalValue);

			nextRun = CorrectStart(worker, nextRun, now);

			if (worker.EndTime != DateTime.MinValue && worker.EndTime.TimeOfDay < nextRun.TimeOfDay)
				nextRun = CorrectTime(nextRun.AddDays(1), worker.StartTime);

			return EnsureValidDate(worker, nextRun);
		}

		private static DateTime CalcNextRunDialy(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;

			var nextRun = initial;

			switch (worker.DayMode)
			{
				case WorkerDayMode.EveryNDay:
					nextRun = nextRun.AddDays(worker.IntervalValue);

					nextRun = CorrectStart(worker, nextRun, now);

					return EnsureValidDate(worker, nextRun);
				case WorkerDayMode.EveryWeekday:
					nextRun = nextRun.AddDays(1);

					for (int i = 0; i < 7; i++)
					{
						if (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
							nextRun = nextRun.AddDays(1);
						else
							break;
					}

					nextRun = CorrectStart(worker, nextRun, now);

					return EnsureValidDate(worker, nextRun);
				default:
					return DateTime.MinValue;
			}
		}

		private static DateTime CalcNextRunWeekly(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;

			var daysIncrement = 7 * (worker.IntervalValue < 1 ? 1 : worker.IntervalValue);
			var nextRun = initial;

			if (!HasWeekdayChecked(worker))
				return nextRun.AddDays(daysIncrement);

			for (int i = 0; i < 7; i++)
			{
				if (IsWeekdayEnabled(worker, nextRun))
					break;

				if (IsWeekCompleted(worker, nextRun) || nextRun.DayOfWeek == DayOfWeek.Sunday)
					nextRun = FixTime(worker, Monday(nextRun).AddDays(daysIncrement));
				else
					nextRun = nextRun.AddDays(1);
			}

			nextRun = CorrectStart(worker, nextRun, now);

			return EnsureValidDate(worker, nextRun);
		}

		private static DateTime CalcNextRunMonthly(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;

			initial = CorrectStart(worker, initial, now);
			var nextRun = FixTime(worker, initial);

			var intervalValue = worker.MonthNumber == 0 ? 1 : worker.MonthNumber;

			nextRun = new DateTime(nextRun.Year, nextRun.Month, 1, nextRun.Hour, nextRun.Minute, nextRun.Second);

			nextRun = ProcessMonthPart(worker, nextRun, worker.MonthMode);

			if (nextRun.Date < now.Date)
			{
				if (worker.MonthMode == WorkerMonthMode.ExactDay)
				{
					nextRun = nextRun.AddMonths(intervalValue);
				}
				else
				{
					nextRun = CalcNextRunMonthly(worker, nextRun.AddMonths(intervalValue), now);
				}
			}

			return nextRun;
		}

		private static DateTime CalcNextRunYearly(IScheduledJob worker, DateTime initial, DateTime now)
		{
			if (HasFinished(worker, now))
				return DateTime.MinValue;
			else
			{
				initial = CorrectStart(worker, initial, now);
				var nextRun = FixTime(worker, initial);

				var intervalValue = worker.IntervalValue < 1 ? 0 : worker.IntervalValue;

				nextRun = new DateTime(nextRun.Year, 1, 1, nextRun.Hour, nextRun.Minute, nextRun.Second);

				switch (worker.YearMode)
				{
					case WorkerYearMode.ExactDate:
						nextRun = EnsureValidDate(worker, new DateTime(nextRun.Year, worker.MonthNumber, worker.DayOfMonth, nextRun.Hour, nextRun.Minute, nextRun.Second));
						break;
					case WorkerYearMode.RelativeDate:
						nextRun = new DateTime(nextRun.Year, worker.MonthNumber, 1, nextRun.Hour, nextRun.Minute, nextRun.Second);
						nextRun = ProcessMonthPart(worker, nextRun, WorkerMonthMode.RelativeDay);
						break;
					default:
						return DateTime.MinValue;
				}

				if (nextRun.Date < now.Date)
				{
					if (worker.YearMode == WorkerYearMode.ExactDate)
					{
						nextRun = nextRun.AddYears(intervalValue);
					}
					else
					{
						nextRun = CalcNextRunYearly(worker, nextRun.AddYears(intervalValue), now);
					}
				}

				return nextRun;
			}
		}

		private static DateTime ProcessMonthPart(IScheduledJob worker, DateTime nextRun, WorkerMonthMode monthMode)
		{
			switch (monthMode)
			{
				case WorkerMonthMode.ExactDay:
					try
					{
						nextRun = new DateTime(nextRun.Year, nextRun.Month, worker.DayOfMonth, nextRun.Hour, nextRun.Minute, nextRun.Second);
					}
					catch
					{
						nextRun = nextRun.AddMonths(1).AddDays(-1);
					}
					break;
				case WorkerMonthMode.RelativeDay:
					switch (worker.MonthPart)
					{
						case WorkerMonthPart.Day:
							switch (worker.IntervalCounter)
							{
								case WorkerCounter.Fourth:
									nextRun = nextRun.AddDays(3);
									break;
								case WorkerCounter.Last:
									nextRun = nextRun.AddMonths(1).AddDays(-1);
									break;
								case WorkerCounter.Second:
									nextRun = nextRun.AddDays(1);
									break;
								case WorkerCounter.Third:
									nextRun = nextRun.AddDays(2);
									break;
							}
							break;
						case WorkerMonthPart.Friday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Friday);
							break;
						case WorkerMonthPart.Monday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Monday);
							break;
						case WorkerMonthPart.Saturday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Saturday);
							break;
						case WorkerMonthPart.Sunday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Sunday);
							break;
						case WorkerMonthPart.Thursday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Thursday);
							break;
						case WorkerMonthPart.Tuesday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Tuesday);
							break;
						case WorkerMonthPart.Wednesday:
							nextRun = TargetDayOfWeek(worker.IntervalCounter, nextRun, DayOfWeek.Wednesday);
							break;
						case WorkerMonthPart.Weekday:
							switch (worker.IntervalCounter)
							{
								case WorkerCounter.First:
									while (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
										nextRun = nextRun.AddDays(1);
									break;
								case WorkerCounter.Fourth:
									var c = 0;

									while (c != 4)
									{
										if (nextRun.DayOfWeek != DayOfWeek.Saturday && nextRun.DayOfWeek != DayOfWeek.Sunday)
											c++;

										nextRun = nextRun.AddDays(1);
									}

									break;
								case WorkerCounter.Last:
									nextRun = nextRun.AddMonths(1).AddDays(-1);

									while (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
										nextRun = nextRun.AddDays(-1);
									break;
								case WorkerCounter.Second:
									var c1 = 0;

									while (c1 != 2)
									{
										if (nextRun.DayOfWeek != DayOfWeek.Saturday && nextRun.DayOfWeek != DayOfWeek.Sunday)
											c1++;

										nextRun = nextRun.AddDays(1);
									}
									break;
								case WorkerCounter.Third:
									var c2 = 0;

									while (c2 != 3)
									{
										if (nextRun.DayOfWeek != DayOfWeek.Saturday && nextRun.DayOfWeek != DayOfWeek.Sunday)
											c2++;

										nextRun = nextRun.AddDays(1);
									}
									break;
							}
							break;
						case WorkerMonthPart.WeekendDay:
							switch (worker.IntervalCounter)
							{
								case WorkerCounter.First:
									while (nextRun.DayOfWeek != DayOfWeek.Saturday && nextRun.DayOfWeek != DayOfWeek.Sunday)
										nextRun = nextRun.AddDays(1);
									break;
								case WorkerCounter.Fourth:
									var c = 0;

									while (c != 4)
									{
										if (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
											c++;

										nextRun = nextRun.AddDays(1);
									}

									break;
								case WorkerCounter.Last:
									nextRun = nextRun.AddMonths(1).AddDays(-1);

									while (nextRun.DayOfWeek != DayOfWeek.Saturday && nextRun.DayOfWeek != DayOfWeek.Sunday)
										nextRun = nextRun.AddDays(-1);
									break;
								case WorkerCounter.Second:
									var c1 = 0;

									while (c1 != 2)
									{
										if (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
											c1++;

										nextRun = nextRun.AddDays(1);
									}
									break;
								case WorkerCounter.Third:
									var c2 = 0;

									while (c2 != 3)
									{
										if (nextRun.DayOfWeek == DayOfWeek.Saturday || nextRun.DayOfWeek == DayOfWeek.Sunday)
											c2++;

										nextRun = nextRun.AddDays(1);
									}
									break;
							}
							break;
					}
					break;
			}

			return EnsureValidDate(worker, nextRun);
		}

		private static DateTime TargetDayOfWeek(WorkerCounter counter, DateTime nextRun, DayOfWeek dayOfWeek)
		{
			switch (counter)
			{
				case WorkerCounter.First:
					while (nextRun.DayOfWeek != dayOfWeek)
						nextRun = nextRun.AddDays(1);
					break;
				case WorkerCounter.Fourth:
					var c = 0;

					while (c != 4)
					{
						if (nextRun.DayOfWeek == dayOfWeek)
							c++;

						if (c != 4)
							nextRun = nextRun.AddDays(1);
					}

					break;
				case WorkerCounter.Last:
					nextRun = nextRun.AddMonths(1).AddDays(-1);

					while (nextRun.DayOfWeek != dayOfWeek)
						nextRun = nextRun.AddDays(-1);
					break;
				case WorkerCounter.Second:
					var c1 = 0;

					while (c1 != 2)
					{
						if (nextRun.DayOfWeek == dayOfWeek)
							c1++;

						if (c1 != 2)
							nextRun = nextRun.AddDays(1);
					}
					break;
				case WorkerCounter.Third:
					var c2 = 0;

					while (c2 != 3)
					{
						if (nextRun.DayOfWeek == dayOfWeek)
							c2++;

						if (c2 != 3)
							nextRun = nextRun.AddDays(1);
					}
					break;
			}

			return nextRun;
		}

		private static bool IsWeekCompleted(IScheduledJob worker, DateTime date)
		{
			var day = date.DayOfWeek;

			switch (day)
			{
				case DayOfWeek.Friday:
					return !(IsWeekdayEnabled(worker, DayOfWeek.Saturday) || IsWeekdayEnabled(worker, DayOfWeek.Sunday));
				case DayOfWeek.Monday:
					return !(IsWeekdayEnabled(worker, DayOfWeek.Tuesday) || IsWeekdayEnabled(worker, DayOfWeek.Wednesday) || IsWeekdayEnabled(worker, DayOfWeek.Thursday) || IsWeekdayEnabled(worker, DayOfWeek.Friday) || IsWeekdayEnabled(worker, DayOfWeek.Saturday) || IsWeekdayEnabled(worker, DayOfWeek.Sunday));
				case DayOfWeek.Saturday:
					return !IsWeekdayEnabled(worker, DayOfWeek.Sunday);
				case DayOfWeek.Sunday:
					return true;
				case DayOfWeek.Thursday:
					return !(IsWeekdayEnabled(worker, DayOfWeek.Friday) || IsWeekdayEnabled(worker, DayOfWeek.Saturday) || IsWeekdayEnabled(worker, DayOfWeek.Sunday));
				case DayOfWeek.Tuesday:
					return !(IsWeekdayEnabled(worker, DayOfWeek.Wednesday) || IsWeekdayEnabled(worker, DayOfWeek.Thursday) || IsWeekdayEnabled(worker, DayOfWeek.Friday) || IsWeekdayEnabled(worker, DayOfWeek.Saturday) || IsWeekdayEnabled(worker, DayOfWeek.Sunday));
				case DayOfWeek.Wednesday:
					return !(IsWeekdayEnabled(worker, DayOfWeek.Thursday) || IsWeekdayEnabled(worker, DayOfWeek.Friday) || IsWeekdayEnabled(worker, DayOfWeek.Saturday) || IsWeekdayEnabled(worker, DayOfWeek.Sunday));
			}

			return true;
		}

		private static bool IsWeekdayEnabled(IScheduledJob worker, DayOfWeek dow)
		{
			switch (dow)
			{
				case DayOfWeek.Friday:
					return (worker.Weekdays & WorkerWeekDays.Friday) == WorkerWeekDays.Friday;
				case DayOfWeek.Monday:
					return (worker.Weekdays & WorkerWeekDays.Monday) == WorkerWeekDays.Monday;
				case DayOfWeek.Saturday:
					return (worker.Weekdays & WorkerWeekDays.Saturday) == WorkerWeekDays.Saturday;
				case DayOfWeek.Sunday:
					return (worker.Weekdays & WorkerWeekDays.Sunday) == WorkerWeekDays.Sunday;
				case DayOfWeek.Thursday:
					return (worker.Weekdays & WorkerWeekDays.Thursday) == WorkerWeekDays.Thursday;
				case DayOfWeek.Tuesday:
					return (worker.Weekdays & WorkerWeekDays.Tuesday) == WorkerWeekDays.Tuesday;
				case DayOfWeek.Wednesday:
					return (worker.Weekdays & WorkerWeekDays.Wednesday) == WorkerWeekDays.Wednesday;
				default:
					return false;
			}
		}

		private static bool IsWeekdayEnabled(IScheduledJob worker, DateTime date)
		{
			switch (date.DayOfWeek)
			{
				case DayOfWeek.Friday:
					return (worker.Weekdays & WorkerWeekDays.Friday) == WorkerWeekDays.Friday;
				case DayOfWeek.Monday:
					return (worker.Weekdays & WorkerWeekDays.Monday) == WorkerWeekDays.Monday;
				case DayOfWeek.Saturday:
					return (worker.Weekdays & WorkerWeekDays.Saturday) == WorkerWeekDays.Saturday;
				case DayOfWeek.Sunday:
					return (worker.Weekdays & WorkerWeekDays.Sunday) == WorkerWeekDays.Sunday;
				case DayOfWeek.Thursday:
					return (worker.Weekdays & WorkerWeekDays.Thursday) == WorkerWeekDays.Thursday;
				case DayOfWeek.Tuesday:
					return (worker.Weekdays & WorkerWeekDays.Tuesday) == WorkerWeekDays.Tuesday;
				case DayOfWeek.Wednesday:
					return (worker.Weekdays & WorkerWeekDays.Wednesday) == WorkerWeekDays.Wednesday;
				default:
					return false;
			}
		}

		private static bool HasWeekdayChecked(IScheduledJob worker)
		{
			return worker.Weekdays != WorkerWeekDays.None;
		}

		private static bool HasFinished(IScheduledJob task, DateTime now)
		{
			switch (task.EndMode)
			{
				case WorkerEndMode.Date:
					return task.EndDate != DateTime.MinValue && task.EndDate < now;
				case WorkerEndMode.NoEnd:
					return false;
				case WorkerEndMode.Occurrence:
					return task.Limit > 0 && task.RunCount >= task.Limit;
			}

			return false;
		}

		private static DateTime FixTime(IScheduledJob worker, DateTime value)
		{
			if (worker.StartTime.TimeOfDay.Ticks == 0)
				return value;

			return CorrectTime(value, worker.StartTime);
		}

		private static DateTime CorrectTime(DateTime date, DateTime time)
		{
			return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
		}

		private static bool IsValidNextRun(IScheduledJob worker, DateTime value)
		{
			if (worker.EndMode == WorkerEndMode.Date && worker.EndDate != DateTime.MinValue && worker.EndDate < value)
				return false;

			return true;
		}

		private static DateTime EnsureValidDate(IScheduledJob worker, DateTime value)
		{
			if (IsValidNextRun(worker, value))
				return value;
			else
				return DateTime.MinValue;
		}

		private static DateTime Monday(DateTime date)
		{
			var diff = date.DayOfWeek - DayOfWeek.Monday;

			if (diff < 0)
				diff += 7;

			return date.AddDays(-1 * diff).Date;
		}
	}
}