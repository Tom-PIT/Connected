using Microsoft.AspNetCore.Hosting;
using TomPIT.Environment;

namespace TomPIT.Runtime
{
	public enum RuntimeEnvironment
	{
		SingleTenant = 1,
		MultiTenant = 2
	}

	public enum EnvironmentStage
	{
		Development = 1,
		Staging = 2,
		Production = 3
	}

	public enum EnvironmentMode
	{
		None = 0,
		Design = 1,
		Runtime = 2,
		Any = 3
	}

	public interface IRuntimeService
	{
		void Initialize(InstanceType type, IWebHostEnvironment environment);
		string ContentRoot { get; }
		string WebRoot { get; }
		RuntimeEnvironment Environment { get; }
		bool SupportsUI { get; }
		InstanceType Type { get; }
		EnvironmentStage Stage { get; }
		EnvironmentMode Mode { get; }
	}

}
