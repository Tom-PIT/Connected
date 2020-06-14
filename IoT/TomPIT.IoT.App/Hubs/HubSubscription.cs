namespace TomPIT.IoT.Hubs
{
	public class HubSubscription : IIoTHubSubscription
	{
		public string MicroService => Name.Split('/')[0];

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
