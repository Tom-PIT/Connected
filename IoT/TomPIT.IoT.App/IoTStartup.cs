﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Connectivity;
using TomPIT.IoT.Hubs;
using TomPIT.IoT.Services;
using TomPIT.Startup;

namespace TomPIT.IoT
{
    public class IoTStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.Booting += OnBooting;
            host.ConfiguringServices += OnConfiguringServices;
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
            e.AddSingleton<IHostedService, FlushingService>();
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(IIoTHubService), typeof(IoTHubService));
        }
    }
}