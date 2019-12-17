using System;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Distributed;
using TomPIT.Middleware;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Deployment
{
	public class InstallerMiddleware : MiddlewareComponent, IInstallerMiddleware
	{
		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}

		protected void RegisterWorker([CIP(CIP.HostedWorkerProvider)]string worker, WorkerInterval interval = WorkerInterval.Hour, int intervalValue = 1, DateTime? startDate = null,
			DateTime? endDate = null, DateTime? startTime = null, DateTime? endTime = null, int limit = 0, int dayOfMonth = 1, WorkerDayMode dayMode = WorkerDayMode.EveryNDay,
			WorkerMonthMode monthMode = WorkerMonthMode.ExactDay, WorkerYearMode yearMode = WorkerYearMode.ExactDate, int monthNumber = 1, WorkerEndMode endMode = WorkerEndMode.NoEnd,
			WorkerCounter intervalCounter = WorkerCounter.First, WorkerMonthPart monthPart = WorkerMonthPart.Day, WorkerWeekDays weekdays = WorkerWeekDays.All)
		{
			var descriptor = ComponentDescriptor.HostedWorker(Context, worker);

			descriptor.Validate();

			var url = Context.Tenant.CreateUrl("Worker", "Select")
				.AddParameter("worker", descriptor.Component.Token);

			var d = Context.Tenant.Get<JObject>(url);

			if (d != null)
				return;

			url = Context.Tenant.CreateUrl("WorkerManagement", "UpdateConfiguration");

			var p = new JObject
			{
				{ "worker" , descriptor.Component.Token },
				{ "startTime" , startTime },
				{ "endTime" , endTime },
				{ "interval" , interval.ToString()},
				{ "intervalValue" , intervalValue },
				{ "startDate" , startDate },
				{ "endDate" , endDate },
				{ "limit" , limit},
				{ "dayOfMonth" , dayOfMonth },
				{ "dayMode" , dayMode.ToString() },
				{ "monthMode" , monthMode.ToString() },
				{ "yearMode" , yearMode.ToString() },
				{ "monthNumber" , monthNumber },
				{ "endMode" , endMode.ToString() },
				{ "intervalCounter" , intervalCounter.ToString() },
				{ "monthPart" , monthPart.ToString() },
				{ "weekdays" , weekdays.ToString() },
				{ "kind" , WorkerKind.Worker.ToString() }
			};

			Context.Tenant.Post(url, p);

			url = Context.Tenant.CreateUrl("WorkerManagement", "Update");

			p = new JObject
			{
				{ "worker" , descriptor.Component.Token },
				{ "status" , WorkerStatus.Enabled.ToString() },
				{ "logging" , false }
			};

			Context.Tenant.Post(url, p);
		}
	}
}
