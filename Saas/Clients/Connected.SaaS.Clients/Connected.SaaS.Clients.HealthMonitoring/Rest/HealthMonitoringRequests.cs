using Connected.SaaS.Clients.HealthMonitoring.Rest.Payloads;
using Connected.SaaS.Clients.Rest;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients.HealthMonitoring.Rest
{
	public class HealthMonitoringRequests : EndpointDefinitionsBase
	{
		public EndpointManagementRequests EndpointManagement { get; }
		public LogManagementRequests LogManagement { get; }
		public MeasurementRequests Measurements { get; }

		private readonly string _subscriptionKey;

		public HealthMonitoringRequests(string subscriptionKey, IRestClient restClient) : base(restClient)
		{
			_subscriptionKey = subscriptionKey;
			EndpointManagement = new EndpointManagementRequests(this);
			LogManagement = new LogManagementRequests(this);
			Measurements = new MeasurementRequests(this);
		}

		public class EndpointManagementRequests
		{
			private readonly HealthMonitoringRequests _requestDispatcher;

			public EndpointManagementRequests(HealthMonitoringRequests requestDispatcher)
			{
				this._requestDispatcher = requestDispatcher;
			}

			public async Task TurnOn(Endpoint endpoint, CancellationToken cancellationToken)
			{
				await _requestDispatcher.PostCall("endpoints/turnOn", GetPayload(endpoint), cancellationToken);
			}
			public async Task TurnOff(Endpoint endpoint, CancellationToken cancellationToken)
			{
				await _requestDispatcher.PostCall("endpoints/turnOff", GetPayload(endpoint), cancellationToken);
			}

			private EndpointManagementPayload GetPayload(Endpoint endpoint)
			{
				return new EndpointManagementPayload
				{
					SubscriptionKey = _requestDispatcher._subscriptionKey,
					EndpointKey = endpoint.Key
				};
			}
		}

		public class LogManagementRequests
		{
			private readonly HealthMonitoringRequests _requestDispatcher;

			public LogManagementRequests(HealthMonitoringRequests requestDispatcher)
			{
				this._requestDispatcher = requestDispatcher;
			}

			public async Task ResolveActiveAlarm(ResolveActiveAlarmPayload e, CancellationToken cancellationToken)
			{
				await _requestDispatcher.PostCall("logs/resolveActiveAlarm", e, cancellationToken);
			}

			public async Task InsertLog(Endpoint endpoint, string message, TraceLevel traceLevel = TraceLevel.Info, CancellationToken cancellationToken = default)
			{
				var payload = new LogInsertPayload
				{
					Level = traceLevel,
					EndpointKey = endpoint.Key,
					Message = message,
					SubscriptionKey = _requestDispatcher._subscriptionKey
				};

				await _requestDispatcher.PostCall("logs/insert", payload, cancellationToken);
			}
		}

		public class MeasurementRequests
		{
			private readonly HealthMonitoringRequests _requestDispatcher;

			public MeasurementRequests(HealthMonitoringRequests requestDispatcher)
			{
				this._requestDispatcher = requestDispatcher;
			}

			public async Task Insert(Endpoint endpoint, int quality, CancellationToken cancellationToken)
			{
				var payload = new MeasurementInsertPayload
				{
					EndpointKey = endpoint.Key,
					Quality = quality,
					SubscriptionKey = _requestDispatcher._subscriptionKey
				};

				await _requestDispatcher.PostCall("measurements/insert", payload, cancellationToken);
			}
		}
	}
}
