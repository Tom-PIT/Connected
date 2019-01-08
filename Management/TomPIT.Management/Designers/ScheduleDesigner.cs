using Newtonsoft.Json.Linq;
using System;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Apis;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Services;

namespace TomPIT.Designers
{
	internal class ScheduleDesigner : DomDesigner<WorkersApiOperationElement>
	{
		private ScheduledJobDescriptor _job = null;

		public ScheduleDesigner(IEnvironment environment, WorkersApiOperationElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Scheduler.cshtml";
		public override object ViewModel
		{
			get
			{
				if (_job == null)
				{
					var op = Owner.Component as IApiOperation;
					var api = op.Closest<IApi>();

					var url = Connection.CreateUrl("Worker", "Select")
						.AddParameter("microService", api.MicroService(Connection))
						.AddParameter("api", api.Component)
						.AddParameter("operation", op.Id);

					var d = Connection.Get<ScheduledJob>(url);

					if (d == null)
					{
						d = new ScheduledJob
						{
							MicroService = api.MicroService(Connection),
							Api = api.Component,
							Operation = op.Id
						};
					}

					_job = new ScheduledJobDescriptor
					{
						Job = d,
						Title = string.Format("{0}/{1}", api.ComponentName(Environment.Context), op.Name)
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
				{ "microService" , d.MicroService },
				{ "api" , d.Api },
				{ "operation" , d.Operation },
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
			d.DayMode = (WorkerDayMode)data.Optional("dayMode", 1);
			d.MonthMode = (WorkerMonthMode)data.Optional("monthMode", 1);
			d.YearMode = (WorkerYearMode)data.Optional("yearMode", 1);
			d.MonthNumber = data.Optional("monthNumber", 1);
			d.EndMode = (WorkerEndMode)data.Optional("endMode", 1);
			d.IntervalCounter = (WorkerCounter)data.Optional("intervalCounter", 1);
			d.MonthPart = (WorkerMonthPart)data.Optional("monthPart", 1);
			d.Weekdays = (WorkerWeekDays)data.Optional("weekdays", 0);
			d.Kind = WorkerKind.Worker;

			var url = Connection.CreateUrl("WorkerManagement", "UpdateConfiguration");

			var p = new JObject
			{
				{ "microService" , d.MicroService },
				{ "api" , d.Api },
				{ "operation" , d.Operation },
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

			var r = new JObject
			{
				{"nextRun",d.NextRun.ToString("G") }
			};

			return Result.JsonResult(this, r);
		}
	}
}
