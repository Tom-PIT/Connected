using System;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Services
{
	internal class Preloader : HostedService
	{
		private HttpConnection _connection = null;

		public Preloader()
		{
			IntervalTimeout = TimeSpan.FromSeconds(45);
		}

		protected override Task Process()
		{
			try
			{
				var ds = DataModel.InstanceEndpoints.Query();

				foreach (var i in ds)
					Load(i);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return Task.CompletedTask;
		}

		private void Load(IInstanceEndpoint endpoint)
		{
			if (string.IsNullOrWhiteSpace(endpoint.Url) || endpoint.Status == InstanceStatus.Disabled)
				return;
			try
			{
				Connection.Get<string>($"{endpoint.Url}/sys/ping");
			}
			catch { }
		}

		private HttpConnection Connection
		{
			get
			{
				if (_connection == null)
					_connection = new HttpConnection();

				return _connection;
			}
		}
	}
}
