﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TomPIT.Environment;
using TomPIT.Proxy;

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

		IApplicationBuilder Host { get; }

		bool IsInitialized { get; }
	}
}
