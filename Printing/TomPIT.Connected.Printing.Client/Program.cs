/*
 * Copyright (c) 2020 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TomPIT.Connected.Printing.Client.Handlers;
using System.ServiceProcess;
using System.Threading.Tasks;
using CommandLine;
using TomPIT.Connected.Printing.Client.Printing;
using DevExpress.XtraReports.Expressions;

namespace TomPIT.Connected.Printing.Client
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Parser.Default.ParseArguments<InstallOptions, UninstallOptions, RunOptions>(args)
                .MapResult(
                async (RunOptions opts) =>
                {
                    await CreateHostBuilder(args).Build().RunAsync();
                    return 0;
                },
                async (InstallOptions opts) =>
                {
                    ServiceManager.Install(opts.Username, opts.Password);
                    Console.WriteLine("Service install completed. Press any key to exit.");
                    Console.ReadLine();
                    return 0;
                },
                async (UninstallOptions opts) =>
                {
                    ServiceManager.Uninstall();
                    Console.WriteLine("Service uninstall completed. Press any key to exit.");
                    Console.ReadLine();
                    return 0;
                },
                errs => Task.FromResult(-1)); // Invalid arguments
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<PrintingHandler>();
                services.AddMemoryCache();
                services.AddSingleton<LocalizationProvider>();
            })         
            .UseWindowsService();

    }
}
