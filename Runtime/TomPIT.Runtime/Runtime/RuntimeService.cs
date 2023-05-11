using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TomPIT.Environment;
using TomPIT.Runtime.Configuration;

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

			var sys = Shell.GetConfiguration<IClientSys>();

			Stage = sys.Stage;
			Connectivity = sys.Connectivity;
			IOBehavior = sys.IOBehavior;
			Platform = sys.Platform;
		}

		public void Initialize(IWebHostEnvironment environment)
		{
			ContentRoot = environment.ContentRootPath;
			WebRoot = environment.WebRootPath;
		}
	}
}
