using Newtonsoft.Json.Linq;
using System;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Workers;
using TomPIT.Dom;
using TomPIT.Services;

namespace TomPIT.Designers
{
	internal class ScheduleDesigner : DomDesigner<DomElement>
	{
		private ScheduledJobDescriptor _job = null;

		public ScheduleDesigner(DomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Scheduler.cshtml";

		private IWorker Worker => Component as IWorker;

		public override object ViewModel
		{
			get
			{
				if (_job == null)
				{
					var worker = Worker;

					var url = Connection.CreateUrl("Worker", "Select")
						.AddParameter("worker", worker.Component);

					var d = Connection.Get<ScheduledJob>(url);

					if (d == null)
					{
						d = new ScheduledJob
						{
							Worker = worker.Component
						};
					}

					_job = new ScheduledJobDescriptor
					{
						Job = d,
						Title = worker.ComponentName(Connection)
					};

					_job.Context = Environment.Context;

					return _job;
				}

				return _job;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (action.Equals("saveConfiguration", StringComparison.OrdinalIgnoreCase))
				return SaveConfiguration(data);
			else if (action.Equals("changeStatus", StringComparison.OrdinalIgnoreCase))
				return ChangeStatus(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult ChangeStatus(JObject data)
		{
			var d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;
			var status = data.Required<WorkerStatus>("status");

			var url = Connection.CreateUrl("WorkerManagement", "Update");

			var p = new JObject
			{
				{ "worker" , d.Worker },
				{ "status" , status.ToString() },
				{ "logging" , d.Logging }
			};

			Connection.Post(url, p);

			_job = null;

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult SaveConfiguration(JObject data)
		{
			var d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;

			d.StartTime = data.Optional("startTime", DateTime.MinValue);
			d.EndTime = data.Optional("endTime", DateTime.MinValue);
			d.Interval = data.Optional("interval", WorkerInterval.Day);
			d.IntervalValue = data.Optional("intervalValue", 1);
			d.StartDate = data.Optional("startDate", DateTime.MinValue);
			d.EndDate = data.Optional("endDate", DateTime.MinValue);
			d.Limit = data.Optional("limit", 0);
			d.DayOfMonth = data.Optional("dayOfMonth", 1);
			d.DayMode = data.Optional("dayMode", WorkerDayMode.EveryNDay);
			d.MonthMode = data.Optional("monthMode", WorkerMonthMode.ExactDay);
			d.YearMode = data.Optional("yearMode", WorkerYearMode.ExactDate);
			d.MonthNumber = data.Optional("monthNumber", 1);
			d.EndMode = data.Optional("endMode", WorkerEndMode.NoEnd);
			d.IntervalCounter = data.Optional("intervalCounter", WorkerCounter.First);
			d.MonthPart = data.Optional("monthPart", WorkerMonthPart.Day);
			d.Weekdays = data.Optional("weekdays", WorkerWeekDays.All);
			d.Kind = WorkerKind.Worker;

			var url = Connection.CreateUrl("WorkerManagement", "UpdateConfiguration");

			var p = new JObject
			{
				{ "worker" , d.Worker },
				{ "startTime" , d.StartTime },
				{ "endTime" , d.EndTime },
				{ "interval" , d.Interval.ToString()},
				{ "intervalValue" , d.IntervalValue },
				{ "startDate" , d.StartDate },
				{ "endDate" , d.EndDate },
				{ "limit" , d.Limit},
				{ "dayOfMonth" , d.DayOfMonth },
				{ "dayMode" , d.DayMode.ToString() },
				{ "monthMode" , d.MonthMode.ToString() },
				{ "yearMode" , d.YearMode.ToString() },
				{ "monthNumber" , d.MonthNumber },
				{ "endMode" , d.EndMode.ToString() },
				{ "intervalCounter" , d.IntervalCounter.ToString() },
				{ "monthPart" , d.MonthPart.ToString() },
				{ "weekdays" , d.Weekdays.ToString() },
				{ "kind" , d.Kind.ToString() }
			};

			Connection.Post(url, p);

			_job = null;
			d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;

			var r = new JObject();

			if (d.NextRun != DateTime.MinValue)
				r.Add("nextRun", d.NextRun.ToString("G"));

			return Result.JsonResult(this, r);
		}
	}
}
