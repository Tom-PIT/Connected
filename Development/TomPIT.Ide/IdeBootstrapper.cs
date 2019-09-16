using TomPIT.Connectivity;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Ide.Analysis.Diagnostics;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Search;
using TomPIT.Ide.VersionControl;

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
			e.Tenant.RegisterService(typeof(IMicroServiceTemplateService), typeof(MicroServiceTemplateService));
			e.Tenant.RegisterService(typeof(IMicroServiceDevelopmentService), typeof(MicroServiceDevelopmentService));
			e.Tenant.RegisterService(typeof(IComponentDevelopmentService), typeof(ComponentDevelopmentService));
			e.Tenant.RegisterService(typeof(ICodeAnalyzerService), typeof(CodeAnalyzerService));
			e.Tenant.RegisterService(typeof(ICodeDiagnosticService), typeof(CodeDiagnosticService));
			e.Tenant.RegisterService(typeof(ICodeAnalysisService), typeof(CodeAnalysisService));
			e.Tenant.RegisterService(typeof(IVersionControlService), typeof(VersionControlService));
			e.Tenant.RegisterService(typeof(IIdeSearchService), typeof(IdeSearchService));
			e.Tenant.RegisterService(typeof(IToolsService), typeof(ToolsService));
		}
	}
}
