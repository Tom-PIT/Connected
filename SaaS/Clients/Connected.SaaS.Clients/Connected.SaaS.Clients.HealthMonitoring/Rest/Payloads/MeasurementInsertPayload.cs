namespace Connected.SaaS.Clients.HealthMonitoring.Rest.Payloads
{
    public class MeasurementInsertPayload : EndpointPayload
    {
        public double Quality { get; set; } = 100;
    }
}
