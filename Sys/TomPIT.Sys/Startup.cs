using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Text;
using TomPIT.Diagnostics;
using TomPIT.Diagnostics.Tracing;
using TomPIT.Serialization;
using TomPIT.Sys.Configuration;
using TomPIT.Sys.Model;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.Services;
using TomPIT.Sys.Workers;
using static TomPIT.HttpExtensions;

namespace TomPIT.Sys
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			Shell.RegisterConfigurationType(typeof(ServerSys));

			services.AddMvc(o =>
			{
				o.EnableEndpointRouting = false;
			}).AddNewtonsoftJson();

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = "TomPIT";
				options.DefaultChallengeScheme = "TomPIT";
				options.DefaultScheme = "TomPIT";
			}).AddTomPITAuthentication("TomPIT", "Tom PIT", o =>
			{

			});

			services.AddAuthorization();

			services.AddSignalR(o =>
			{
				o.EnableDetailedErrors = true;
			});


			RegisterTasks(services);

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseTomPITExceptionMiddleware();
			}
			else
			{
				app.UseTomPITExceptionMiddleware();
				//app.UseExceptionHandler();
			}

            RegisterTraceService(app);

            app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(routes =>
			{
				routes.MapHub<CacheHub>("/caching");
				routes.MapHub<IoTHub>("/iot");
				routes.MapHub<BigDataHub>("/bigdata");
				routes.MapHub<DataCacheHub>("/datacaching");
				routes.MapHub<TraceHub>("hubs/tracing");
            });

			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller}/{action}");
			});

			ServerConfiguration.Initialize(app);
			Shell.Configure(app);
			DataModel.Initialized = true;

        }

		private void RegisterTraceService(IApplicationBuilder app) 
		{
            var traceHubContext = app.ApplicationServices.GetRequiredService<IHubContext<TraceHub>>();

            var traceService = app.ApplicationServices.GetService<ITraceService>();

            traceService.TraceReceived += async (s, e) => await TraceHub.Trace(traceHubContext, e);

            traceService.AddEndpoint("TomPIT.Sys.Diagnostics", "IncomingRequest");
            traceService.AddEndpoint("TomPIT.Sys.Diagnostics", "LongLastingRequest");
			traceService.AddEndpoint("TomPIT.Sys.Diagnostics", "UserControllerRequest");

            app.Use(async (context, next) => {
				context.Request.EnableBuffering();
                var path = context.Request.Path;
                var stopwatch = Stopwatch.StartNew();
			
                var body = context.Request.Body.ToJObject();
                traceService.Trace("TomPIT.Sys.Diagnostics", "IncomingRequest", $"{path} {Serializer.Serialize(body)} {Serializer.Serialize(context.Request.Query)}");

				if (path.HasValue && path.Value.ToLower().Contains("user"))
				{
					var sourceData = new
					{
						SourceIp = context.Connection?.RemoteIpAddress,
						SourcePort = context.Connection?.RemotePort,
						User = context.User?.Identity?.Name,
						UserAuthenticated = context.User?.Identity?.IsAuthenticated
					};

                    var userRequestMessage = $"Path: {path} Payload: {Serializer.Serialize(body)}, QueryParams: {Serializer.Serialize(context.Request.Query)}, Source: {Serializer.Serialize(sourceData)}";
 					traceService.Trace("TomPIT.Sys.Diagnostics", "UserControllerRequest", userRequestMessage);
				}

                if (next is not null && !context.Response.HasStarted)
                    await next();

                if (stopwatch.ElapsedMilliseconds > 2000)
                    traceService.Trace("TomPIT.Sys.Diagnostics", "LongLastingRequest", path);
            });
        }

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<ITraceService, TraceService>();
			services.AddSingleton<IHostedService, MessageDispatcher>();
			services.AddSingleton<IHostedService, MessageDisposer>();
			services.AddSingleton<IHostedService, Scheduler>();
			services.AddSingleton<IHostedService, Preloader>();
			services.AddSingleton<IHostedService, QueuePersistence>();
			services.AddSingleton<IHostedService, EventsPersistence>();
			services.AddSingleton<IHostedService, PrintingPersistence>();
			services.AddSingleton<IHostedService, PrintingSpoolerPersistence>();
			services.AddSingleton<IHostedService, BigDataBufferPersistence>();
		}
	}
}
