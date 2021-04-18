namespace TomPIT.IoT.Hubs
{
	public class IoTHubDevice : IIoTHubDevice
	{
		public string Name { get; set; }

		public string MicroService => Name.Split('/')[0];
		public string Hub => Name.Split('/')[1];
		public override string ToString()
		{
			return Name;
		}
	}
}
