using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Worker.HostedServices
{
	internal class HostedServicesContainer : TenantObject, IHostedServices
	{
		private ConcurrentDictionary<Guid, IHostedServiceMiddleware> _items = null;
		private bool _initialized = false;
		private CancellationTokenSource _tokenSource = null;

		public HostedServicesContainer(ITenant tenant) : base(tenant)
		{
			_tokenSource = new CancellationTokenSource();
		}

		public void Initialize()
		{
			if (_initialized)
				return;

			_initialized = true;

			var lifetime = Tenant.GetService<IRuntimeService>().Host.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

			lifetime.ApplicationStopping.Register(() =>
			{
				_tokenSource.Cancel();
			});

			_items = new ConcurrentDictionary<Guid, IHostedServiceMiddleware>();

			Tenant.GetService<IComponentService>().ConfigurationAdded += async (sender, args) => { await OnConfigurationAdded(sender, args); };
			Tenant.GetService<IComponentService>().ConfigurationChanged += async (sender, args) => { await OnConfigurationChanged(sender, args); };
			Tenant.GetService<IComponentService>().ConfigurationRemoved += async (sender, args) => { await OnConfigurationRemoved(sender, args); };

			foreach (var service in Tenant.GetService<IComponentService>().QueryComponents(ComponentCategories.HostedService))
				AsyncUtils.RunSync(() => InvalidateService(service.MicroService, service.Token));
		}

		private async Task OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.HostedService, true) == 0)
				await InvalidateService(e.MicroService, e.Component);
		}

		private async Task OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.HostedService, true) == 0)
				await InvalidateService(e.MicroService, e.Component);
		}

		private async Task OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.HostedService, true) == 0)
				await InvalidateService(e.MicroService, e.Component);
		}

		private async Task InvalidateService(Guid microService, Guid component)
		{
			if (Items.TryGetValue(component, out IHostedServiceMiddleware existing))
				await existing.Stop(CancellationToken.None);

			if (Tenant.GetService<IComponentService>().SelectConfiguration(component) is not IHostedServiceConfiguration config)
				return;

			if (Tenant.GetService<IMicroServiceService>().Select(microService) is not IMicroService ms)
				return;

			if (Tenant.GetService<ICompilerService>().ResolveType(ms.Token, config, config.ComponentName(), false) is not Type type)
				return;

			using var ctx = new MicroServiceContext(ms);

			if (Tenant.GetService<ICompilerService>().CreateInstance<IHostedServiceMiddleware>(ctx, type) is not IHostedServiceMiddleware middleware)
				return;

			Items.TryAdd(component, middleware);

			await middleware.Start(_tokenSource.Token);
		}

		private ConcurrentDictionary<Guid, IHostedServiceMiddleware> Items => _items;
	}
}
