namespace TomPIT.Net
{
	public class InstanceEndpointUrl : ServerUrl
	{
		public InstanceEndpointUrl(string instance)
		{
			Instance = instance;
		}

		protected override string BaseUrl { get { return Instance; } }
		public string Instance { get; private set; }

		public static InstanceEndpointUrl CreateEndpointUrl(string instance, string controller, string action)
		{
			var r = new InstanceEndpointUrl(instance)
			{
				Controller = controller,
				Action = action
			};

			return r;
		}
	}
}
