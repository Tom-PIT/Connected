using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using TomPIT.ComponentModel;
using TomPIT.Environment;

namespace TomPIT.Runtime
{
	internal class RuntimeService : IRuntimeService
	{
		internal static IApplicationBuilder _host;
		private List<Assembly> _microServices;

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
			Features = Instance.Features;

			if (Features.HasFlag(InstanceFeatures.Management) || Features.HasFlag(InstanceFeatures.Development) || Features.HasFlag(InstanceFeatures.Application))
				SupportsUI = true;
			else if (Features.HasFlag(InstanceFeatures.Development))
				Mode = EnvironmentMode.Design;
			else if (Features.HasFlag(InstanceFeatures.Application))
				SupportsUI = true;

			if (Shell.Configuration.RootElement.TryGetProperty("stage", out JsonElement stageElement))
				Stage = Enum.Parse<EnvironmentStage>(stageElement.GetString());

			if (Shell.Configuration.RootElement.TryGetProperty("connectivity", out JsonElement connectivityElement))
				Connectivity = Enum.Parse<EnvironmentConnectivity>(connectivityElement.GetString());

			if (Shell.Configuration.RootElement.TryGetProperty("ioBehavior", out JsonElement ioElement))
				IOBehavior = Enum.Parse<EnvironmentIOBehavior>(ioElement.GetString());

			if (Shell.Configuration.RootElement.TryGetProperty("platform", out JsonElement platformElement))
				Platform = Enum.Parse<Platform>(platformElement.GetString());

			if (Shell.Configuration.RootElement.TryGetProperty("optimization", out JsonElement optimizationElement))
				Optimization = Enum.Parse<EnvironmentOptimization>(optimizationElement.GetString());
		}

		public void Initialize(IWebHostEnvironment environment)
		{
			ContentRoot = environment.ContentRootPath;
			WebRoot = environment.WebRootPath;
		}

		public bool IsMicroServiceSupported(Guid microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms is null)
				return false;

			return Stage switch
			{
				EnvironmentStage.Development => ms.SupportedStages.HasFlag(MicroServiceStages.Development),
				EnvironmentStage.QualityAssurance => ms.SupportedStages.HasFlag(MicroServiceStages.QualityAssurance),
				EnvironmentStage.Staging => ms.SupportedStages.HasFlag(MicroServiceStages.Staging),
				EnvironmentStage.Production => ms.SupportedStages.HasFlag(MicroServiceStages.Production),
				_ => throw new NotSupportedException(),
			};
		}
	}
}
