using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Services;
using TomPIT.Development.CodeAnalysis.SnippetProviders;

namespace TomPIT
{
	public static class DevelopmentBootstrapper
	{
		public static void Run()
		{
			Shell.GetService<IConnectivityService>().ConnectionInitialize += OnConnectionInitialize;
			Shell.GetService<IConnectivityService>().ConnectionInitialized += OnConnectionInitialized;
		}

		private static void OnConnectionInitialized(object sender, SysConnectionArgs e)
		{
			e.Connection.GetService<ICodeAnalysisService>().RegisterSnippetProvider(new DataCommandParametersProvider());
			e.Connection.GetService<ICodeAnalysisService>().RegisterSnippetProvider(new EntityImportProvider());
		}

		private static void OnConnectionInitialize(object sender, SysConnectionArgs e)
		{
			e.Connection.RegisterService(typeof(IQaService), typeof(QaService));
		}
	}
}
