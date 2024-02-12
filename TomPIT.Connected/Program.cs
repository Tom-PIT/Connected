using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Connected
{
	public class Program
	{
		private static object _lastException = new();
		private static bool _startedErrorServer = false;
		private static IHost _host;

		public static async Task Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledExceptionThrown;

			try
			{
				var builder = WebApplication.CreateBuilder(args);
				var boot = new Startup();

				boot.ConfigureServices(builder.Services);

				var webApp = builder.Build();

				_host = webApp;

				boot.Configure(webApp, builder.Environment);

				foreach (var startup in MicroServices.Startups)
					await startup.Initialize(_host);

				foreach (var startup in MicroServices.Startups)
					await startup.Start();

				await _host.RunAsync();
			}
			finally
			{
				if (_startedErrorServer)
				{
					while (true)
						System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private static void OnUnhandledExceptionThrown(object sender, UnhandledExceptionEventArgs e)
		{
			StartErrorServer(e.ExceptionObject);
		}

		private static void StartErrorServer(object exception)
		{
			_lastException = exception;

			if (_startedErrorServer)
				return;

			_startedErrorServer = true;

			var app = WebApplication.CreateBuilder().Build();
			app.MapGet("/shutdown", () => { System.Environment.Exit(100); });
			app.MapGet("{**catchAll}", (httpContext) =>
			{
				var html = $"""
<a href="/shutdown">Shutdown instance</a>
<div>Error starting application:</div>
<div><code>{_lastException}</code></div>
""";
				httpContext.Response.ContentType = MediaTypeNames.Text.Html;
				httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(html);
				httpContext.Response.StatusCode = 500;
				return httpContext.Response.WriteAsync(html);

			});

			app.Run();
		}
	}
}
