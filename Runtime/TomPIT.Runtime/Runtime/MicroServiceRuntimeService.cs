using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Runtime;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Runtime
{
    internal class MicroServiceRuntimeService : SynchronizedClientRepository<IRuntimeMiddleware, Guid>, IMicroServiceRuntimeService
    {
        public MicroServiceRuntimeService(ITenant tenant) : base(tenant, "runtimeMiddleware")
        {
            Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
            Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
            Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
            Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
        }

        private IApplicationBuilder Host { get; set; }
        private IServiceCollection Services { get; set; }

        public void Configure(IApplicationBuilder app)
        {
            Host = app;

            foreach (var runtime in QueryRuntimes())
                runtime.Initialize(new RuntimeInitializeArgs(app));
        }

        public void Configure(IServiceCollection services)
        {
            Services = services;

            Initialize();
        }

        protected override void OnInitializing()
        {
            var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(ComponentCategories.Runtime);

            foreach (var i in configurations)
                LoadRuntime(i as IRuntimeConfiguration);
        }

        protected override void OnInvalidate(Guid id)
        {
            LoadRuntime(Tenant.GetService<IComponentService>().SelectConfiguration(id) as IRuntimeConfiguration);
        }
        private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
        {
            if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
                return;

            Refresh(e.Component);
        }

        private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
        {
            if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
                return;

            Refresh(e.Component);
        }

        private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
        {
            if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
                return;

            Remove(e.Component);
        }

        private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
        {
            if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
                return;

            Refresh(e.Component);
        }

        private void LoadRuntime(IRuntimeConfiguration config)
        {
            if (config is null)
                return;

            if (Tenant.GetService<ICompilerService>().ResolveType(config.MicroService(), config, config.ComponentName(), false) is not Type type)
                return;

            using var ctx = new MicroServiceContext(config.MicroService());

            try
            {
                var instance = Tenant.GetService<ICompilerService>().CreateInstance<IRuntimeMiddleware>(ctx, type);

                if (instance is not null)
                {
                    if (Services is not null)
                        instance.Configure(Services);

                    if (Host is not null)
                        instance.Initialize(new RuntimeInitializeArgs(Host));

                    Set(config.Component, instance, TimeSpan.Zero);
                }
            }
            catch
            {
                //nothing to do here
            }
        }

        public ImmutableList<IRuntimeMiddleware> QueryRuntimes()
        {
            return All();
        }
    }
}
