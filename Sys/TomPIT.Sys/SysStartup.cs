using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Configuration;
using System.Diagnostics;
using System.Text.Json;

using TomPIT.Diagnostics;
using TomPIT.Diagnostics.Tracing;
using TomPIT.Environment;
using TomPIT.Serialization;
using TomPIT.Startup;
using TomPIT.Sys.Configuration;
using TomPIT.Sys.Exceptions;
using TomPIT.Sys.Model;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.Services;
using TomPIT.Sys.Workers;

namespace TomPIT.Sys
{
   public class SysStartup : IStartupClient
   {
      public void Initialize(IStartupHost host)
      {
         host.Booting += OnBooting;
         host.ConfiguringMvc += OnConfiguringMvc;
         host.ConfiguringSignalR += OnConfiguringSignalR;
         host.ConfiguringRouting += OnConfiguringRouting;
         host.ConfiguringMvcRouting += OnConfiguringMvcRouting;
         host.Configuring += OnConfiguring;
         host.ConfiguringServices += ConfigureServices;
      }

      private void OnBooting(object sender, System.EventArgs e)
      {
         ServerConfiguration.Initialize();
         DatabaseInitializer.Initialize();
      }

      private void OnConfiguring(object sender, System.Tuple<IApplicationBuilder, IWebHostEnvironment> e)
      {
         RegisterTraceService(e.Item1);
         DataModel.Initialized = true;
      }

      private void OnConfiguringMvcRouting(object sender, Microsoft.AspNetCore.Routing.IRouteBuilder e)
      {
         e.MapRoute("default", "{controller}/{action}");
      }

      private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
      {
         e.MapHub<CacheHub>("/caching");
         e.MapHub<IoTHub>("/iot");
         e.MapHub<BigDataHub>("/bigdata");
         e.MapHub<DataCacheHub>("/datacaching");
         e.MapHub<TraceHub>("hubs/tracing");
      }

      private void OnConfiguringSignalR(object sender, HubOptions e)
      {
         e.AddFilter<ExceptionHubFilter>();
      }

      private void OnConfiguringMvc(object sender, Microsoft.AspNetCore.Mvc.MvcOptions e)
      {
         e.EnableEndpointRouting = false;
      }

      public void ConfigureServices(object sender, IServiceCollection services)
      {
         services.AddSingleton<ExceptionHubFilter>();

         RegisterTasks(services);
      }

      private void RegisterTraceService(IApplicationBuilder app)
      {
         var traceHubContext = app.ApplicationServices.GetRequiredService<IHubContext<TraceHub>>();

         var traceService = app.ApplicationServices.GetService<ITraceService>();

         traceService.TraceReceived += async (s, e) => await TraceHub.Trace(traceHubContext, e);

         traceService.AddEndpoint("TomPIT.Sys.Diagnostics", "IncomingRequest");
         traceService.AddEndpoint("TomPIT.Sys.Diagnostics", "LongLastingRequest");
         traceService.AddEndpoint("TomPIT.Sys.Diagnostics", "UserControllerRequest");

         app.Use(async (context, next) =>
         {
            context.Request.EnableBuffering();

            var path = context.Request.Path;
            var stopwatch = Stopwatch.StartNew();

            var body = context.Request.Body.ToJObject();

            traceService.Trace("TomPIT.Sys.Diagnostics", "IncomingRequest", $"{path} {Serializer.Serialize(body)} {Serializer.Serialize(context.Request.Query)}");

            if (path.HasValue && path.Value.ToLower().Contains("user"))
            {
               var sourceData = new
               {
                  SourceIp = context.Connection?.RemoteIpAddress?.ToString(),
                  SourcePort = context.Connection?.RemotePort.ToString(),
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

         if (Shell.Configuration.RootElement.TryGetProperty("sys", out JsonElement element))
         {
            if (element.TryGetProperty("usePreloader", out JsonElement preloaderElement))
            {
               if(preloaderElement.GetBoolean())
                  services.AddSingleton<IHostedService, Preloader>();
            }
         }


         services.AddSingleton<IHostedService, QueuePersistence>();
         services.AddSingleton<IHostedService, EventsPersistence>();
         services.AddSingleton<IHostedService, PrintingPersistence>();
         services.AddSingleton<IHostedService, PrintingSpoolerPersistence>();
         services.AddSingleton<IHostedService, BigDataBufferPersistence>();
      }
   }
}
