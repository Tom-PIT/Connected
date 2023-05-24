using Microsoft.AspNetCore.Builder;
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
using TomPIT.Security;
using TomPIT.Startup;
using TomPIT.UI;
using TomPIT.UI.Theming;

namespace TomPIT.App
{
    public class AppStartup : IStartupClient
    {
        internal static IEndpointRouteBuilder RouteBuilder = null;

        public void Initialize(IStartupHost host)
        {
            host.ConfiguringServices += OnConfigureServices;
            host.ConfiguringSignalR += OnConfigureSignalR;
            host.MvcConfigured += OnMvcConfigured;
            host.ConfiguringRouting += OnConfiguringRouting;
            host.Configuring += OnConfiguring;
            host.Booting += OnBooting;
            host.ConfigureEmbeddedStaticResources += OnConfigureEmbeddedStaticResources;
        }

        private void OnConfigureEmbeddedStaticResources(object sender, System.Collections.Generic.List<System.Reflection.Assembly> e)
        {
            e.Add(typeof(AppStartup).Assembly);
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnConfiguring(object sender, System.Tuple<IApplicationBuilder, IWebHostEnvironment> e)
        {
            e.Item1.UseMiddleware<IgnoreRouteMiddleware>();

            var traceHubContext = e.Item1.ApplicationServices.GetRequiredService<IHubContext<TraceHub>>();

            var traceService = e.Item1.ApplicationServices.GetService<ITraceService>();

            traceService.TraceReceived += async (s, ea) => await TraceHub.Trace(traceHubContext, ea);

            traceService.AddEndpoint("TomPIT.App.diagnostics", "IncomingRequest");
            traceService.AddEndpoint("TomPIT.App.diagnostics", "LongLastingRequest");

            e.Item1.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                var stopwatch = Stopwatch.StartNew();

                traceService.Trace("TomPIT.App.diagnostics", "IncomingRequest", path);

                if (next is not null && !context.Response.HasStarted)
                    await next.Invoke();

                if (stopwatch.ElapsedMilliseconds > 2000)
                    traceService.Trace("TomPIT.App.diagnostics", "LongLastingRequest", path);
            });

            AppRouting.RegisterRouteMiddleware(e.Item1);
        }

        private void OnConfiguringRouting(object sender, IEndpointRouteBuilder e)
        {
            e.MapHub<TraceHub>("hubs/tracing");

            AppRouting.Register(e);
        }

        private void OnMvcConfigured(object sender, IMvcBuilder e)
        {
            e.AddRazorRuntimeCompilation(opts =>
                {
                    opts.FileProviders.Add(new ViewProvider());
                }
            );
        }

        private void OnConfigureSignalR(object sender, HubOptions e)
        {
            e.EnableDetailedErrors = true;
        }

        private void OnConfigureServices(object sender, IServiceCollection e)
        {
            e.AddResponseCompression(o =>
            {
                o.EnableForHttps = true;
                o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                          new[] { "image/svg+xml", "image/png", "image/jpg", "image/jpeg" });
            });

            //e.Configure<KeyManagementOptions>(opts =>
            //{
            //	opts.XmlEncryptor = new XmlKeyEncryptor();
            //	opts.XmlRepository = new XmlKeyRepository();
            //});

            //e.AddDataProtection();

            e.AddAntiforgery(o =>
            {
                o.Cookie.Name = "TomPITAntiForgery";
                o.FormFieldName = "TomPITAntiForgery";
                o.HeaderName = "X-TP-AF";
                o.SuppressXFrameOptionsHeader = false;
            });

            e.AddScoped<IViewEngine, ViewEngine>();
            e.AddScoped<IMailTemplateViewEngine, MailTemplateViewEngine>();

            e.Configure<RazorViewEngineOptions>(opts =>
                 {
                     opts.ViewLocationExpanders.Add(new ViewLocationExpander());
                 }
            );

            e.AddSingleton<ITraceService, TraceService>();
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(IXmlKeyService), typeof(XmlKeyService));
            e.Tenant.RegisterService(typeof(IViewService), typeof(ViewService));
            e.Tenant.RegisterService(typeof(IThemeService), typeof(ThemeService), true);
            e.Tenant.RegisterService(typeof(IResourceService), typeof(ResourceService));
            e.Tenant.RegisterService(typeof(IViewArgumentService), typeof(ViewArgumentService));
            e.Tenant.RegisterService(typeof(IClientGlobalizationService), typeof(ClientGlobalizationService));
        }
    }
}
