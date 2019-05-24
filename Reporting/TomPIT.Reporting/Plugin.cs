using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Configuration;
using TomPIT.Reporting.Storage;

namespace TomPIT.Reporting
{
	internal class Plugin : IPlugin
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDevExpressControls();

			services.ConfigureReportingServices(configurator => {
				//configurator.ConfigureReportDesigner(designerConfigurator => {
				//	designerConfigurator.RegisterDataSourceWizardConfigFileConnectionStringsProvider();
				//});
				configurator.ConfigureWebDocumentViewer(viewerConfigurator => {
					viewerConfigurator.UseCachedReportSourceBuilder();
				});
			});
		}

		public List<string> GetApplicationParts()
		{
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
			var reportDirectory = Path.Combine(env.ContentRootPath, "Reports");

			DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new ReportStorage(reportDirectory));
			DevExpress.XtraReports.Configuration.Settings.Default.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.Expressions;

			app.UseDevExpressControls();

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
			//builder.MapRoute("sys/plugins/iot/partial/{id}", (t) =>
			//{
			//	return Task.CompletedTask;
			//});

//			builder.MapRoute("sys.plugins.iot.partial", "sys/plugins/iot/partial/{microService}/{view}", new { controller = "IoT", action = "Partial" }, null, new { Namespace = "TomPIT.IoT.Controllers" });
		}
	}
}
