using Microsoft.AspNetCore.Hosting;
using TomPIT.Net;

namespace TomPIT.Runtime
{
	internal class RuntimeService : IRuntimeService
	{
		public string ContentRoot { get; set; }
		public string WebRoot { get; set; }
		public RuntimeEnvironment Environment { get; set; } = RuntimeEnvironment.SingleTenant;
		public bool SupportsUI { get; set; }
		public InstanceType Type { get; set; }
		public EnvironmentStage Stage { get; set; }
		public EnvironmentMode Mode { get; set; } = EnvironmentMode.Runtime;

		public void Initialize(InstanceType type, IHostingEnvironment environment)
		{
			Type = type;
			ContentRoot = environment.ContentRootPath;
			WebRoot = environment.WebRootPath;

			switch (Type)
			{
				case InstanceType.Unknown:
					break;
				case InstanceType.Management:
					SupportsUI = true;
					Environment = RuntimeEnvironment.MultiTenant;
					break;
				case InstanceType.Development:
					SupportsUI = true;
					Environment = RuntimeEnvironment.MultiTenant;
					Mode = EnvironmentMode.Design;
					break;
				case InstanceType.Application:
					SupportsUI = true;
					break;
				case InstanceType.Worker:
					break;
				case InstanceType.Cdn:
					break;
				case InstanceType.IoT:
					break;
				case InstanceType.BigData:
					break;
				case InstanceType.Search:
					break;
				case InstanceType.Rest:
					break;
				case InstanceType.Test:
					break;
				default:
					break;
			}

			if (environment.IsEnvironment(EnvironmentName.Development))
				Stage = EnvironmentStage.Development;
			else if (environment.IsEnvironment(EnvironmentName.Staging))
				Stage = EnvironmentStage.Staging;
			else
				Stage = EnvironmentStage.Production;
		}
	}
}
