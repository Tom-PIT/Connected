namespace Connected.SaaS.Clients.HealthMonitoring.Rest.Payloads
{
    public class ResolveActiveAlarmPayload : EndpointPayload
    {
        public string ConfirmedByType { get; set; }
        public string ConfirmedByKey { get; set; }
    }
}
