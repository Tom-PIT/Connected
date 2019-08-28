using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.IoT;
using TomPIT.Resources;
using TomPIT.Themes;
using TomPIT.UI;

namespace TomPIT
{
	public class Startup
	{
		internal static IRouteBuilder RouteBuilder = null;
		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.SingleTenant
			};

			services.AddAntiforgery(o =>
			{
				o.Cookie.Name = "TomPITAntiForgery";
				o.FormFieldName = "TomPITAntiForgery";
				o.HeaderName = "X-TP-AF";
				o.SuppressXFrameOptionsHeader = false;
			});

			services.AddResponseCompression(o =>
			{
				o.EnableForHttps = true;
				o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					 new[] { "image/svg+xml", "image/png", "image/jpg", "image/jpeg" });
			});

			Instance.Initialize(services, e);

			services.AddScoped<IViewEngine, ViewEngine>();
			services.AddScoped<IMailTemplateViewEngine, MailTemplateViewEngine>();
			
			services.Configure<RazorViewEngineOptions>(opts =>
			{
				opts.ViewLocationExpanders.Add(new ViewLocationExpander());

				opts.FileProviders.Add(
					new ViewProvider()
			  );
			}
			);
		}


		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseResponseCompression();
			Instance.Configure(InstanceType.Application, app, env, (f) =>
			{
				app.UseMiddleware<IgnoreRouteMiddleware>();

				RouteBuilder = f.Builder;
				Configuration.Routing.Register(f.Builder);
			});

			InitializeConfiguration();
			Instance.Run(app);
		}

		private void InitializeConfiguration()
		{
			Shell.GetService<IConnectivityService>().ConnectionInitializing += OnConnectionInitializing;
		}

		private void OnConnectionInitializing(object sender, SysConnectionArgs e)
		{
			e.Connection.RegisterService(typeof(IViewService), typeof(ViewService));
			e.Connection.RegisterService(typeof(IThemeService), typeof(ThemeService));
			e.Connection.RegisterService(typeof(IResourceService), typeof(ResourceService));
			e.Connection.RegisterService(typeof(IClientGlobalizationService), typeof(ClientGlobalizationService));
		}
	}
}
