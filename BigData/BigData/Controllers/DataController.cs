using Amt.DataHub.Data;
using Amt.Sdk.Filters;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;

namespace Amt.DataHub.Controllers
{
	[ServerAuthentication]
	public class DataController : ApiController
	{
		[HttpPost]
		public DataTable Query([FromBody] Command command)
		{
			if (command == null)
				return null;

			var parameters = new List<Tuple<string, object>>();

			if (command.Parameters != null)
			{
				foreach (var i in command.Parameters)
					parameters.Add(new Tuple<string, object>(i.Name, i.Value == null ? null : i.Value.Value));

			}

			return DataModel.Query(command.CommandText, parameters);
		}

		[HttpPost]
		public DataTable ProcessTask(Guid taskId, int timeout)
		{
			return DataHubProxy.ProcessTask(taskId, timeout);
		}

		[HttpPost]
		public void CancelTask(Guid popReceipt, string connectionId)
		{
			DataHubProxy.CancelTask(popReceipt, connectionId);
		}

		[HttpPost]
		public void CompleteTask(Guid popReceipt)
		{
			DataHubProxy.CompleteTask(popReceipt);
		}
	}
}