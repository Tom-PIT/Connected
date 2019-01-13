using Amt.Api;
using Amt.DataHub;
using Amt.DataHub.Partitions;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Filters;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace Amt.Server.DataHub
{
	[ServerAuthentication]
	public class PartitionHandler : DelegatingHandler
	{
		private IHttpControllerSelector _controllerSelector;
		private readonly HttpConfiguration _configuration;

		public PartitionHandler(HttpConfiguration configuration)
		{
			_configuration = configuration;
		}

		public HttpConfiguration Configuration
		{
			get { return _configuration; }
		}

		private IHttpControllerSelector ControllerSelector
		{
			get
			{
				if (_controllerSelector == null)
					_controllerSelector = _configuration.Services.GetHttpControllerSelector();

				return _controllerSelector;
			}
		}


		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			IHttpRouteData routeData = request.GetRouteData();

			var partition = new Guid(routeData.Values["partition"] as string);
			var task = new Guid(routeData.Values["task"] as string);
			var response = request.CreateResponse(System.Net.HttpStatusCode.OK);

			var par = AmtShell.GetService<IConfigurationService>().Select<PartitionConfiguration>(partition);

			if (par == null)
				response.StatusCode = System.Net.HttpStatusCode.NotFound;
			else
			{
				var tsk = DataHubModel.SelectTransactionTask(task);

				if (tsk == null || tsk.Status != TransactionTaskStatus.Busy)
					response.StatusCode = System.Net.HttpStatusCode.NotFound;
				else
				{
					try
					{
						ProcessContent(par, tsk, request);
					}
					catch (Exception ex)
					{
						response.StatusCode = System.Net.HttpStatusCode.BadRequest;
						response.ReasonPhrase = ex.Message;
					}
				}
			}

			var t = new TaskCompletionSource<HttpResponseMessage>();

			t.SetResult(response);

			return t.Task;
		}

		private void ProcessContent(PartitionConfiguration config, TransactionTask task, HttpRequestMessage request)
		{
			var content = RestUtils.DeserializeBody(request);

			if (string.IsNullOrWhiteSpace(content))
				return;

			var dataTable = JsonConvert.DeserializeObject<DataTable>(content);

			if (dataTable == null || dataTable.Rows.Count == 0)
				return;

			PartitionModel.Update(config.RefId, task.Id, dataTable, task.PopReceipt);
		}
	}
}