using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Environment;

namespace TomPIT.Runtime
{
	internal class RuntimeService : IRuntimeService
	{
		internal static IApplicationBuilder _host;

		private ConcurrentDictionary<Guid, MicroServiceInfo> InfoMap { get; }
		public ImmutableArray<Assembly> RecompiledMicroServices => MicroServiceCompiler.Compiled;
		public ImmutableArray<Assembly> MicroServices => TomPIT.MicroServices.Assemblies;
		public bool IsHotSwappingSupported => Stage != EnvironmentStage.Development;
		public string ContentRoot { get; set; }
		public string WebRoot { get; set; }
		public RuntimeEnvironment Environment { get; set; } = RuntimeEnvironment.SingleTenant;
		public bool SupportsUI { get; set; }
		public InstanceFeatures Features { get; set; }
		public EnvironmentStage Stage { get; set; }
		public EnvironmentMode Mode { get; set; } = EnvironmentMode.Runtime;
		public EnvironmentOptimization Optimization { get; set; } = EnvironmentOptimization.Release;
		public IApplicationBuilder Host => _host;

		public Platform Platform { get; private set; } = Platform.Cloud;

		public EnvironmentConnectivity Connectivity { get; private set; }

		public EnvironmentIOBehavior IOBehavior { get; private set; } = EnvironmentIOBehavior.ReadWrite;
		public bool IsInitialized { get; internal set; }

		public RuntimeService()
		{
			InfoMap = new();
			Features = Instance.Features;

			if (Features.HasFlag(InstanceFeatures.Management) || Features.HasFlag(InstanceFeatures.Development) || Features.HasFlag(InstanceFeatures.Application))
				SupportsUI = true;
			else if (Features.HasFlag(InstanceFeatures.Development))
				Mode = EnvironmentMode.Design;
			else if (Features.HasFlag(InstanceFeatures.Application))
				SupportsUI = true;

			Shell.Configuration.Bind(this);
		}

		public void Initialize(IWebHostEnvironment environment)
		{
			ContentRoot = environment.ContentRootPath;
			WebRoot = environment.WebRootPath;
		}

		public bool IsMicroServiceSupported(Guid microService)
		{
			if (Stage != EnvironmentStage.Development)
				return true;

			if (InfoMap.TryGetValue(microService, out MicroServiceInfo existing) && existing is not null)
				return existing.SupportsDevelopment;

			var sourceText = ResolveMicroServiceInfoText(microService);

			if (sourceText is null)
			{
				InfoMap.TryAdd(microService, new MicroServiceInfo());

				return false;
			}

			var info = JsonSerializer.Deserialize<MicroServiceInfo>(sourceText, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			if (info is null)
			{
				InfoMap.TryAdd(microService, new MicroServiceInfo());

				return false;
			}

			InfoMap.TryAdd(microService, info);

			return info.SupportsDevelopment;
		}

		private static string? ResolveMicroServiceInfoText(Guid microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms is null)
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(microService, ComponentCategories.Text, "MicroService.json");

			if (component is null)
				return null;

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as ITextConfiguration;

			if (config is null)
				return null;

			return Tenant.GetService<IComponentService>().SelectText(microService, config);
		}
	}
}
