using TomPIT.Connectivity;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.TextServices;
using TomPIT.Ide.UI.Theming;
using TomPIT.UI.Theming;

namespace TomPIT.Ide
{
	public static class IdeBootstrapper
	{
		public static void Run()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private static void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IDesignerService), typeof(DesignerService));
			e.Tenant.RegisterService(typeof(IMicroServiceDevelopmentService), typeof(MicroServiceDevelopmentService));
			//e.Tenant.RegisterService(typeof(ICodeAnalyzerService), typeof(CodeAnalyzerService));
			//e.Tenant.RegisterService(typeof(ICodeDiagnosticService), typeof(CodeDiagnosticService));
			//e.Tenant.RegisterService(typeof(ICodeAnalysisService), typeof(CodeAnalysisService));
			e.Tenant.RegisterService(typeof(IToolsService), typeof(ToolsService));
			e.Tenant.RegisterService(typeof(ITextService), typeof(TextService));
			e.Tenant.RegisterService(typeof(IStylesheetService), typeof(StylesheetService));
			e.Tenant.RegisterService(typeof(IThemeService), typeof(ThemeService));
		}
	}
}
