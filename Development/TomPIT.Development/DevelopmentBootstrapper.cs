using TomPIT.Connectivity;
using TomPIT.Development.Navigation;
using TomPIT.Development.Quality;

namespace TomPIT.Development
{
	public static class DevelopmentBootstrapper
	{
		public static void Run()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private static void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IQualityService), typeof(QualityService));
			e.Tenant.RegisterService(typeof(INavigationDesignService), typeof(NavigationDesignService));
		}
	}
}
