using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.App.Globalization;
using TomPIT.App.Resources;
using TomPIT.App.Routing;
using TomPIT.App.UI;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.UI;
using TomPIT.UI.Theming;

namespace TomPIT.App
{
	public class Startup
	{
		internal static IEndpointRouteBuilder RouteBuilder = null;
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
			}
			);

			Instance.Mvc.AddRazorRuntimeCompilation(
						(opts) =>
						{
							opts.FileProviders.Add(new ViewProvider());
						});
		}


		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();
			Instance.Configure(InstanceType.Application, app, env, (f) =>
			{
				app.UseMiddleware<IgnoreRouteMiddleware>();

				RouteBuilder = f.Builder;
				AppRouting.Register(app, f.Builder);
			});

			InitializeConfiguration();
			Instance.Run(app);
		}

		private void InitializeConfiguration()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IViewService), typeof(ViewService));
			e.Tenant.RegisterService(typeof(IThemeService), typeof(ThemeService));
			e.Tenant.RegisterService(typeof(IResourceService), typeof(ResourceService));
			e.Tenant.RegisterService(typeof(IClientGlobalizationService), typeof(ClientGlobalizationService));
		}
	}
}
