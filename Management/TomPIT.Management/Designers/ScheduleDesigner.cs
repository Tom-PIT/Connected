using System;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Design.Ide.Designers;
using TomPIT.Distributed;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Management.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Management.Designers
{
	internal class ScheduleDesigner : DomDesigner<DomElement>
	{
		private ScheduledJobDescriptor _job = null;

		public ScheduleDesigner(DomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Scheduler.cshtml";

		private IWorkerConfiguration Worker => Component as IWorkerConfiguration;

		public override object ViewModel
		{
			get
			{
				if (_job == null)
				{
					var worker = Worker;

					var url = Environment.Context.Tenant.CreateUrl("Worker", "Select")
						.AddParameter("worker", worker.Component);

					var d = Environment.Context.Tenant.Get<ScheduledJob>(url);

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
						Title = worker.ComponentName()
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
			else if (action.Equals("reset", StringComparison.OrdinalIgnoreCase))
				return Reset(data);
			else if (action.Equals("run", StringComparison.OrdinalIgnoreCase))
				return Run(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Reset(JObject data)
		{
			var d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;

			var url = Environment.Context.Tenant.CreateUrl("WorkerManagement", "Reset");

			var p = new JObject
			{
				{ "worker" , d.Worker }
			};

			Environment.Context.Tenant.Post(url, p);

			_job = null;

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult Run(JObject data)
		{
			var d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;

			var url = Environment.Context.Tenant.CreateUrl("WorkerManagement", "Run");

			var p = new JObject
			{
				{ "worker" , d.Worker }
			};

			Environment.Context.Tenant.Post(url, p);

			_job = null;

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult ChangeStatus(JObject data)
		{
			var d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;
			var status = data.Required<WorkerStatus>("status");

			var url = Environment.Context.Tenant.CreateUrl("WorkerManagement", "Update");

			var p = new JObject
			{
				{ "worker" , d.Worker },
				{ "status" , status.ToString() },
				{ "logging" , d.Logging }
			};

			Environment.Context.Tenant.Post(url, p);

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

			var url = Environment.Context.Tenant.CreateUrl("WorkerManagement", "UpdateConfiguration");

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

			Environment.Context.Tenant.Post(url, p);

			_job = null;
			d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;

			var r = new JObject();

			if (d.NextRun != DateTime.MinValue)
				r.Add("nextRun", d.NextRun.ToString("G"));

			return Result.JsonResult(this, r);
		}
	}
}
