using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System.Diagnostics;
using System.Linq;

namespace TomPIT.App
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if (args.Any(e=> string.Compare("WaitForDebugger", e, true) == 0))
                while (!Debugger.IsAttached) 
				{
					System.Threading.Thread.Sleep(200);
				}

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				  .ConfigureWebHostDefaults(webBuilder =>
				  {
					  webBuilder.UseStartup<Startup>();
				  });
	}
}
