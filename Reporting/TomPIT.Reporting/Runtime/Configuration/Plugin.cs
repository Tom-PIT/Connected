using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.XtraReports.Expressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Cdn.Documents;
using TomPIT.MicroServices.Reporting.Printing;
using TomPIT.MicroServices.Reporting.Storage;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.MicroServices.Reporting.Runtime.Configuration
{
	internal class Plugin : IPlugin
	{
		public void ConfigureServices(IServiceCollection services)
		{
         if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            DevExpress.Drawing.Internal.DXDrawingEngine.ForceSkia();
         
         services.AddDevExpressControls();

			services.ConfigureReportingServices(configurator =>
			{
				configurator.ConfigureReportDesigner(designerConfigurator =>
				{
					designerConfigurator.RegisterDataSourceWizardConfigFileConnectionStringsProvider();
				});
				configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
				{
					viewerConfigurator.UseCachedReportSourceBuilder();
				});
			});
		}

		public List<string> GetApplicationParts(ApplicationPartManager manager)
		{
			var parts = manager.ApplicationParts;
			var aspNetCoreReportingAssemblyName = typeof(DevExpress.AspNetCore.Reporting.WebDocumentViewer.WebDocumentViewerController).Assembly.GetName().Name;
			var reportingPart = parts.FirstOrDefault(part => part.Name == aspNetCoreReportingAssemblyName);

			if (reportingPart != null)
				parts.Remove(reportingPart);

			return new List<string>
			{
				"TomPIT.MicroServices.Reporting",
				//"TomPIT.MicroServices.Reporting.Views"
			};
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>
			{
				"TomPIT.MicroServices.Reporting"
			};
		}

		public List<IDocumentProvider> GetDocumentProviders()
		{
			return new List<IDocumentProvider>
			{
				new DocumentProvider()
			};
		}

		public void Initialize(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Runtime)
				DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new ReportRuntimeStorage());

			DevExpress.XtraReports.Configuration.Settings.Default.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.ExpressionsAdvanced;

			app.UseDevExpressControls();

			CustomFunctions.Register(new LocalizeFunction());
		}

		public void RegisterRoutes(IEndpointRouteBuilder builder)
		{
			builder.MapControllerRoute("sys.reporting.viewer", "DXXRDV", new { controller = "ReportViewer", action = "Invoke" }, null, new { Namespace = "TomPIT.MicroServices.Reporting.Controllers" });
			builder.MapControllerRoute("sys.reporting.querybuilder", "DXXQB", new { controller = "ReportQueryBuilder", action = "Invoke" }, null, new { Namespace = "TomPIT.MicroServices.Reporting.Controllers" });
			builder.MapControllerRoute("sys.reporting.designer", "DXXRD", new { controller = "Designer", action = "Invoke" }, null, new { Namespace = "TomPIT.MicroServices.Reporting.Controllers" });
		}
	}
}
