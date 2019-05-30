using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Configuration;
using TomPIT.Reporting.Storage;
using TomPIT.Services;

namespace TomPIT.Reporting
{
	internal class Plugin : IPlugin
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDevExpressControls();

			services.ConfigureReportingServices(configurator => {
				configurator.ConfigureReportDesigner(designerConfigurator =>
				{
					designerConfigurator.RegisterDataSourceWizardConfigFileConnectionStringsProvider();
				});
				configurator.ConfigureWebDocumentViewer(viewerConfigurator => {
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
				"TomPIT.Reporting",
				"TomPIT.Reporting.Views"
			};
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>
			{
				"TomPIT.Reporting"
			};
		}

		public void Initialize(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Runtime)
				DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new ReportRuntimeStorage());

			DevExpress.XtraReports.Configuration.Settings.Default.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.ExpressionsAdvanced;

			app.UseDevExpressControls();
		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
			//builder.MapRoute("sys/plugins/iot/partial/{id}", (t) =>
			//{
			//	return Task.CompletedTask;
			//});

			builder.MapRoute("sys.reporting.viewer", "DXXRDV", new { controller = "ReportViewer", action = "Invoke" }, null, new { Namespace = "TomPIT.Reporting.Controllers" });
			builder.MapRoute("sys.reporting.querybuilder", "DXXQB", new { controller = "ReportQueryBuilder", action = "Invoke" }, null, new { Namespace = "TomPIT.Reporting.Controllers" });
			builder.MapRoute("sys.reporting.designer", "DXXRD", new { controller = "Designer", action = "Invoke" }, null, new { Namespace = "TomPIT.Reporting.Controllers" });
		}
	}
}
