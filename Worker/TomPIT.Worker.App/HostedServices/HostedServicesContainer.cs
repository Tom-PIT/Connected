using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
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
			/*
			 * Invalidations of the same component are serialized, otherwise two concurrent ones (startup
			 * plus a configuration event, or two quick saves) would both start an instance and one of them
			 * would never be stopped.
			 */
			var sync = Locks.GetOrAdd(component, f => new SemaphoreSlim(1, 1));

			await sync.WaitAsync();

			try
			{
				if (Items.TryRemove(component, out IHostedServiceMiddleware existing))
					await existing.Stop(CancellationToken.None);

				if (Tenant.GetService<IComponentService>().SelectConfiguration(component) is not IHostedServiceConfiguration config)
					return;

				if (Tenant.GetService<IMicroServiceService>().Select(microService) is not IMicroService ms)
					return;

				if (Tenant.GetService<ICompilerService>().ResolveType(ms.Token, config, config.ComponentName(), false) is not Type type)
					return;

				var ctx = new MicroServiceContext(ms);

				if (Tenant.GetService<ICompilerService>().CreateInstance<IHostedServiceMiddleware>(ctx, type) is not IHostedServiceMiddleware middleware)
				{
					ctx.Dispose();
					return;
				}

				Items[component] = middleware;
				/*
				 * Start runs for the lifetime of the service (recurring services never return), so it must
				 * not be awaited here, otherwise Initialize blocks on the first recurring service and the
				 * services after it never start.
				 */
				_ = Task.Run(() => Run(middleware, ctx));
			}
			catch (Exception ex)
			{
				Tenant.LogError(nameof(HostedServicesContainer), ex.Message);
			}
			finally
			{
				sync.Release();
			}
		}

		private async Task Run(IHostedServiceMiddleware middleware, MicroServiceContext ctx)
		{
			try
			{
				await middleware.Start(_tokenSource.Token);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex)
			{
				Tenant.LogError(nameof(HostedServicesContainer), ex.Message);
			}
			finally
			{
				ctx.Dispose();
			}
		}

		private ConcurrentDictionary<Guid, IHostedServiceMiddleware> Items => _items;
		private ConcurrentDictionary<Guid, SemaphoreSlim> Locks { get; } = new();
	}
}
