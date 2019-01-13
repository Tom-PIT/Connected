using Amt.Api;
using Amt.DataHub;
using Amt.DataHub.Endpoints;
using Amt.Sdk.DataHub;
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
	public class DataHubEndpointHandler : DelegatingHandler
	{
		private IHttpControllerSelector _controllerSelector;
		private readonly HttpConfiguration _configuration;

		public DataHubEndpointHandler(HttpConfiguration configuration)
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

			var endpoint = routeData.Values["endpoint"] as string;
			var response = request.CreateResponse(System.Net.HttpStatusCode.OK);

			var component = EndpointsModel.Select(endpoint);

			if (component == null)
				response.StatusCode = System.Net.HttpStatusCode.NotFound;
			else
			{
				if (component.Status == EndpointStatus.Disabled)
				{
					Log.Warning(this, "Endpoint is disabled.", LogEvents.DhEndpointDisabled, endpoint);

					response.StatusCode = System.Net.HttpStatusCode.NotFound;
				}

				if (response.IsSuccessStatusCode)
				{
					try
					{
						ProcessContent(request, component);
					}
					catch (Exception ex)
					{
						Log.Error(this, ex, LogEvents.DhEndpointRequestError, endpoint);

						response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
					}
				}
			}

			var task = new TaskCompletionSource<HttpResponseMessage>();

			task.SetResult(response);

			return task.Task;
		}

		private void ProcessContent(HttpRequestMessage request, Endpoint endpoint)
		{
			var content = RestUtils.DeserializeBody(request);

			if (string.IsNullOrWhiteSpace(content))
				return;

			var dataTable = JsonConvert.DeserializeObject<DataTable>(content);

			if (dataTable == null)
			{
				Log.Warning(this, "Could not deserialize body to DataTable.", LogEvents.DhDeserializeError, endpoint.Url);
				return;
			}

			EndpointsModel.Push(endpoint.Url, dataTable);
		}
	}
}