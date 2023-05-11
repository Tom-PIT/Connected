using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;
using TomPIT.App.Globalization;
using TomPIT.App.Resources;
using TomPIT.App.Routing;
using TomPIT.App.UI;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagnostics.Tracing;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.UI;
using TomPIT.UI.Theming;

namespace TomPIT.App
{
	public class AppStartup : IInstanceStartup
	{
		internal static IEndpointRouteBuilder RouteBuilder = null;

		private IApplicationBuilder App { get; set; }
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddResponseCompression(o =>
			{
				o.EnableForHttps = true;
				o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
						  new[] { "image/svg+xml", "image/png", "image/jpg", "image/jpeg" });
			});

			services.Configure<KeyManagementOptions>(opts =>
			{
				opts.XmlEncryptor = new XmlKeyEncryptor();
				opts.XmlRepository = new XmlKeyRepository();
			});

			services.AddDataProtection();

			services.AddAntiforgery(o =>
			{
				o.Cookie.Name = "TomPITAntiForgery";
				o.FormFieldName = "TomPITAntiForgery";
				o.HeaderName = "X-TP-AF";
				o.SuppressXFrameOptionsHeader = false;
			});

			services.AddScoped<IViewEngine, ViewEngine>();
			services.AddScoped<IMailTemplateViewEngine, MailTemplateViewEngine>();

			services.AddSignalR(o =>
			{
				o.EnableDetailedErrors = true;
			}).AddNewtonsoftJsonProtocol();

			services.Configure<RazorViewEngineOptions>(opts =>
				 {
					 opts.ViewLocationExpanders.Add(new ViewLocationExpander());
				 }
			);

			Instance.Mvc.AddRazorRuntimeCompilation(opts =>
				 {
					 opts.FileProviders.Add(new ViewProvider());
				 }
			);

			services.AddSingleton<ITraceService, TraceService>();

			if (MiddlewareDescriptor.Current?.Tenant?.GetService<IMicroServiceRuntimeService>() is IMicroServiceRuntimeService runtimeService)
				runtimeService.Configure(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			App = app;

			app.UseResponseCompression();
		}

		public void ConfigureRouting(ConfigureRoutingArgs e)
		{
			App.UseMiddleware<IgnoreRouteMiddleware>();

			RouteBuilder = e.Builder;

			e.Builder.MapHub<TraceHub>("hubs/tracing");

			AppRouting.Register(e.Builder);
		}

		public void ConfigureMiddleware(ConfigureMiddlewareArgs e)
		{
			var traceHubContext = e.Builder.ApplicationServices.GetRequiredService<IHubContext<TraceHub>>();

			var traceService = e.Builder.ApplicationServices.GetService<ITraceService>();

			traceService.TraceReceived += async (s, ea) => await TraceHub.Trace(traceHubContext, ea);

			traceService.AddEndpoint("TomPIT.App.diagnostics", "IncomingRequest");
			traceService.AddEndpoint("TomPIT.App.diagnostics", "LongLastingRequest");

			e.Builder.Use(async (context, next) =>
			{
				var path = context.Request.Path;
				var stopwatch = Stopwatch.StartNew();

				traceService.Trace("TomPIT.App.diagnostics", "IncomingRequest", path);

				if (next is not null && !context.Response.HasStarted)
					await next.Invoke();

				if (stopwatch.ElapsedMilliseconds > 2000)
					traceService.Trace("TomPIT.App.diagnostics", "LongLastingRequest", path);
			});

			AppRouting.RegisterRouteMiddleware(e.Builder);
		}

		public void Initialize()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IXmlKeyService), typeof(XmlKeyService));
			e.Tenant.RegisterService(typeof(IViewService), typeof(ViewService));
			e.Tenant.RegisterService(typeof(IThemeService), typeof(ThemeService));
			e.Tenant.RegisterService(typeof(IResourceService), typeof(ResourceService));
			e.Tenant.RegisterService(typeof(IViewArgumentService), typeof(ViewArgumentService));
			e.Tenant.RegisterService(typeof(IClientGlobalizationService), typeof(ClientGlobalizationService));
		}
	}
}
