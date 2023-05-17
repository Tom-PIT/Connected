using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Text.Json;
using TomPIT.Environment;

namespace TomPIT.Runtime
{
	internal class RuntimeService : IRuntimeService
	{
		internal static IApplicationBuilder _host;

		public string ContentRoot { get; set; }
		public string WebRoot { get; set; }
		public RuntimeEnvironment Environment { get; set; } = RuntimeEnvironment.SingleTenant;
		public bool SupportsUI { get; set; }
		public InstanceFeatures Features { get; set; }
		public EnvironmentStage Stage { get; set; }
		public EnvironmentMode Mode { get; set; } = EnvironmentMode.Runtime;

		public IApplicationBuilder Host => _host;

		public Platform Platform { get; private set; } = Platform.Cloud;

		public EnvironmentConnectivity Connectivity { get; private set; }

		public EnvironmentIOBehavior IOBehavior { get; private set; } = EnvironmentIOBehavior.ReadWrite;
		public bool IsInitialized { get; internal set; }

		public RuntimeService()
		{
			Features = Instance.Features;

			if (Features.HasFlag(InstanceFeatures.Management | InstanceFeatures.Development))
			{
				SupportsUI = true;
				Environment = RuntimeEnvironment.MultiTenant;
			}
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
		}

		public void Initialize(IWebHostEnvironment environment)
		{
			ContentRoot = environment.ContentRootPath;
			WebRoot = environment.WebRootPath;
		}
	}
}
