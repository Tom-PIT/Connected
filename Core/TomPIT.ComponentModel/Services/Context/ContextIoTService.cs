namespace TomPIT.Services.Context
{
	internal class ContextIoTService : ContextClient, IContextIoTService
	{
		private string _server = null;

		public ContextIoTService(IExecutionContext context) : base(context)
		{

		}

		public string Server
		{
			get
			{
				if (_server == null)
				{
					_server = Context.Services.Routing.GetServer(Environment.InstanceType.IoT, Environment.InstanceVerbs.All);

					if (_server == null)
						throw new RuntimeException(SR.ErrNoIoTServer);

					_server = $"{_server}/iot";
				}

				return _server;
			}
		}
	}
}
