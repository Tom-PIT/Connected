/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

#if CONSOLE
using System;
using System.Threading;
using TomPIT.Connected.Printing.Client.Handlers;
#else
using System.ServiceProcess;
#endif

namespace TomPIT.Connected.Printing.Client
{
    class Program
	{
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
