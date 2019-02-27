using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Design.Services;
using TomPIT.Ide.Design.VersionControl;

namespace TomPIT
{
	public static class IdeBootstrapper
	{
		public static void Run()
		{
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
		}

		private static void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(IDesignerService), typeof(DesignerService));
			e.Connection.RegisterService(typeof(IMicroServiceTemplateService), typeof(MicroServiceTemplateService));
			e.Connection.RegisterService(typeof(IMicroServiceDevelopmentService), typeof(MicroServiceDevelopmentService));
			e.Connection.RegisterService(typeof(IComponentDevelopmentService), typeof(ComponentDevelopmentService));
			e.Connection.RegisterService(typeof(ICodeCompletionService), typeof(CodeCompletionService));
			e.Connection.RegisterService(typeof(ICodeDiagnosticService), typeof(CodeDiagnosticService));
			e.Connection.RegisterService(typeof(ICodeAnalysisService), typeof(CodeAnalysisService));
			e.Connection.RegisterService(typeof(IVersionControlService), typeof(VersionControlService));
		}
	}
}
