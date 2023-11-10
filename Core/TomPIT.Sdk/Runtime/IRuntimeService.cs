using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Immutable;
using System.Reflection;
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
		QualityAssurance = 2,
		Staging = 3,
		Production = 4
	}

	public enum EnvironmentMode
	{
		None = 0,
		Design = 1,
		Runtime = 2,
		Any = 3
	}

	public enum Platform
	{
		OnPrem = 1,
		Cloud = 2
	}

	public enum EnvironmentConnectivity
	{
		Online = 1,
		Offline = 2
	}

	public enum EnvironmentIOBehavior
	{
		NotSet = 0,
		ReadWrite = 1,
		ReadOnly = 2
	}

	public enum EnvironmentOptimization
	{
		Debug = 1,
		Release = 2
	}

	public interface IRuntimeService
	{
		void Initialize(IWebHostEnvironment environment);
		string ContentRoot { get; }
		string WebRoot { get; }
		RuntimeEnvironment Environment { get; }
		bool SupportsUI { get; }
		InstanceFeatures Features { get; }
		EnvironmentStage Stage { get; }
		EnvironmentMode Mode { get; }
		Platform Platform { get; }
		EnvironmentConnectivity Connectivity { get; }
		EnvironmentIOBehavior IOBehavior { get; }
		EnvironmentOptimization Optimization { get; }

		IApplicationBuilder Host { get; }

		bool IsInitialized { get; }
		bool IsHotSwappingSupported { get; }
		bool IsMicroServiceSupported(Guid microService);
		ImmutableArray<Assembly> RecompiledMicroServices { get; }
	}
}
