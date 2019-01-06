using System;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Workers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Workers;

namespace TomPIT.Design
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

					var url = SysContext.CreateUrl("Worker", "Select")
						.AddParameter("microService", api.MicroService(SysContext))
						.AddParameter("api", api.Component)
						.AddParameter("operation", op.Id);

					var d = SysContext.Connection.Get<ScheduledJob>(url);

					if (d == null)
					{
						d = new ScheduledJob
						{
							MicroService = api.MicroService(SysContext),
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
			var status = data.Argument<WorkerStatus>("status");

			var url = SysContext.CreateUrl("WorkerManagement", "Update");

			var p = new JObject
			{
				{ "microService" , d.MicroService },
				{ "api" , d.Api },
				{ "operation" , d.Operation },
				{ "status" , status.ToString() },
				{ "logging" , d.Logging }
			};

			SysContext.Connection.Post(url, p);

			_job = null;

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult SaveConfiguration(JObject data)
		{
			var d = (ViewModel as ScheduledJobDescriptor).Job as ScheduledJob;

			d.StartTime = data.Argument("startTime", DateTime.MinValue);
			d.EndTime = data.Argument("endTime", DateTime.MinValue);
			d.Interval = data.Argument("interval", WorkerInterval.Day);
			d.IntervalValue = data.Argument("intervalValue", 1);
			d.StartDate = data.Argument("startDate", DateTime.MinValue);
			d.EndDate = data.Argument("endDate", DateTime.MinValue);
			d.Limit = data.Argument("limit", 0);
			d.DayOfMonth = data.Argument("dayOfMonth", 1);
			d.DayMode = (WorkerDayMode)data.Argument("dayMode", 1);
			d.MonthMode = (WorkerMonthMode)data.Argument("monthMode", 1);
			d.YearMode = (WorkerYearMode)data.Argument("yearMode", 1);
			d.MonthNumber = data.Argument("monthNumber", 1);
			d.EndMode = (WorkerEndMode)data.Argument("endMode", 1);
			d.IntervalCounter = (WorkerCounter)data.Argument("intervalCounter", 1);
			d.MonthPart = (WorkerMonthPart)data.Argument("monthPart", 1);
			d.Weekdays = (WorkerWeekDays)data.Argument("weekdays", 0);
			d.Kind = WorkerKind.Worker;

			var url = SysContext.CreateUrl("WorkerManagement", "UpdateConfiguration");

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

			SysContext.Connection.Post(url, p);

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
