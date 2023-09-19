using System.Diagnostics;

namespace Connected.SaaS.Clients.HealthMonitoring.Rest.Payloads
{
	public class LogInsertPayload : EndpointPayload
	{
		public TraceLevel Level { get; set; } = TraceLevel.Info;
		public string Message { get; set; }
	}
}
