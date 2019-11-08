using TomPIT.Connectivity;
using TomPIT.Development.Analysis.SnippetProviders;
using TomPIT.Development.Navigation;
using TomPIT.Development.Quality;
using TomPIT.Ide.Analysis;

namespace TomPIT.Development
{
	public static class DevelopmentBootstrapper
	{
		public static void Run()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Shell.GetService<IConnectivityService>().TenantInitialized += OnTenantInitialized;
		}

		private static void OnTenantInitialized(object sender, TenantArgs e)
		{
			e.Tenant.GetService<ICodeAnalysisService>().RegisterSnippetProvider(new DataCommandParametersProvider());
			e.Tenant.GetService<ICodeAnalysisService>().RegisterSnippetProvider(new EntityImportProvider());
		}

		private static void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IQualityService), typeof(QualityService));
			e.Tenant.RegisterService(typeof(INavigationDesignService), typeof(NavigationDesignService));
		}
	}
}
