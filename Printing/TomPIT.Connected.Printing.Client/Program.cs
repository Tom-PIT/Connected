using System;
using System.Text;
#if CONSOLE
using TomPIT.Connected.Printing.Client.Handlers;
#else
using System.ServiceProcess;
#endif

namespace TomPIT.Connected.Printing.Client
{
    class Program
	{
		private static readonly string Token = Convert.ToBase64String(Encoding.UTF8.GetBytes("c00588c4-a3e8-4ed4-95e8-e995ba18ab5f"));
		private static readonly string Cdn = "http://localhost:44018";

		static void Main(string[] args)
		{
#if CONSOLE
			var printingHandler = new PrintingHandler();
			printingHandler.Start();
			Console.ReadKey();
			printingHandler.Stop();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PrinterSpoolerService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
		}

	}
}
