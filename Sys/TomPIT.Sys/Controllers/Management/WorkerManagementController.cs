using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Distributed;
using TomPIT.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class WorkerManagementController : SysController
	{
		[HttpPost]
		public List<IQueueMessage> Dequeue()
		{
			var body = FromBody();

			var count = body.Required<int>("count");

			var r = new List<IQueueMessage>();

			return DataModel.Workers.Dequeue(count);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Workers.Ping(microService, popReceipt);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Workers.Complete(microService, popReceipt);
		}

		[HttpPost]
		public void Error()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Workers.Error(microService, popReceipt);
		}

		[HttpPost]
		public void UpdateConfiguration()
		{
			var body = FromBody();

			var worker = body.Required<Guid>("worker");
			var startTime = body.Optional("startTime", DateTime.MinValue);
			var endTime = body.Optional("endTime", DateTime.MinValue);
			var interval = body.Required<WorkerInterval>("interval");
			var intervalValue = body.Optional("intervalValue", 1);
			var startDate = body.Optional("startDate", DateTime.MinValue);
			var endDate = body.Optional("endDate", DateTime.MinValue);
			var limit = body.Optional("limit", 0);
			var dayOfMonth = body.Optional("dayOfMonth", 1);
			var dayMode = body.Optional("dayMode", WorkerDayMode.EveryNDay);
			var monthMode = body.Optional("monthMode", WorkerMonthMode.ExactDay);
			var yearMode = body.Optional("yearMode", WorkerYearMode.ExactDate);
			var monthNumber = body.Optional("monthNumber", 1);
			var endMode = body.Optional("endMode", WorkerEndMode.NoEnd);
			var intervalCounter = body.Optional("intervalCounter", WorkerCounter.First);
			var monthPart = body.Optional("monthPart", WorkerMonthPart.Weekday);
			var weekdays = body.Optional("weekdays", WorkerWeekDays.None);
			var kind = body.Optional("kind", WorkerKind.Worker);

			DataModel.Workers.Update(worker, startTime, endTime, interval, intervalValue, startDate, endDate, limit, dayOfMonth,
				dayMode, monthMode, yearMode, monthNumber, endMode, intervalCounter, monthPart, weekdays, kind);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var worker = body.Required<Guid>("worker");
			var status = body.Required<WorkerStatus>("status");
			var logging = body.Required<bool>("logging");

			DataModel.Workers.Update(worker, status, logging);
		}

		[HttpPost]
		public void Reset()
		{
			var body = FromBody();

			var worker = body.Required<Guid>("worker");

			DataModel.Workers.Reset(worker);
		}

		[HttpPost]
		public void Run()
		{
			var body = FromBody();

			var worker = body.Required<Guid>("worker");

			DataModel.Workers.Run(worker);
		}

		[HttpPost]
		public void AttachState()
		{
			var body = FromBody();

			var worker = body.Required<Guid>("worker");
			var state = body.Required<Guid>("state");

			DataModel.Workers.UpdateState(worker, state);
		}
	}
}
