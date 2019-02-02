using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT
{
	public static class DevelopmentBootstrapper
	{
		public static void Run()
		{
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
		}

		private static void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(IQaService), typeof(QaService));
		}
	}
}
