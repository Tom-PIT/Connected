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
        public static void Initialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(IDesignerService), typeof(DesignerService), true);
            e.Tenant.RegisterService(typeof(IMicroServiceDevelopmentService), typeof(MicroServiceDevelopmentService), true);
            //e.Tenant.RegisterService(typeof(ICodeAnalyzerService), typeof(CodeAnalyzerService));
            //e.Tenant.RegisterService(typeof(ICodeDiagnosticService), typeof(CodeDiagnosticService));
            //e.Tenant.RegisterService(typeof(ICodeAnalysisService), typeof(CodeAnalysisService));
            e.Tenant.RegisterService(typeof(IToolsService), typeof(ToolsService), true);
            e.Tenant.RegisterService(typeof(ITextService), typeof(TextService), true);
            e.Tenant.RegisterService(typeof(IStylesheetService), typeof(StylesheetService), true);
            e.Tenant.RegisterService(typeof(IThemeService), typeof(ThemeService), true);
        }
    }
}
